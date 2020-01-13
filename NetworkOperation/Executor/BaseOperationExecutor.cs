using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.ObjectPool;
using NetworkOperation.Dispatching;
using NetworkOperation.Extensions;
using NetworkOperation.Logger;

namespace NetworkOperation
{
    public abstract class BaseOperationExecutor<TRequest, TResponse> : IResponseReceiver<TResponse>, IGlobalCancellation where TResponse : IOperationMessage, new() where TRequest : IOperationMessage, new()
         {
        private struct OperationId : IEquatable<OperationId>
        {
            public OperationId(int id, uint code)
            {
                Id = id;
                Code = code;
            }
            public bool Equals(OperationId other)
            {
                return Id == other.Id && Code == other.Code;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is OperationId other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (Id * 397) ^ (int) Code;
                }
            }

            public readonly int Id;
            public readonly uint Code;
            
        }
        
        private readonly ConcurrentDictionary<OperationId, Task> _responseQueue = new ConcurrentDictionary<OperationId, Task>();

        private readonly BaseSerializer _serializer;
        
        private static readonly ObjectPool<State> StatesPool = new DefaultObjectPool<State>(new DefaultPooledObjectPolicy<State>());
        private readonly Side _currentSide;
        private readonly Session _session;
        private readonly SessionCollection _sessions;
        private readonly IStructuralLogger _logger;


        public CancellationToken GlobalToken { get; set; }
        
        protected BaseOperationExecutor(OperationRuntimeModel model, BaseSerializer serializer, SessionCollection sessions, IStructuralLogger logger)
        {
            Model = model;
            _serializer = serializer;
            _sessions = sessions;
            _logger = logger;
            _currentSide = Side.Server;
        }

        protected BaseOperationExecutor(OperationRuntimeModel model, BaseSerializer serializer, Session session, IStructuralLogger logger)
        {
            
            Model = model;
            _serializer = serializer;
            _session = session;
            _logger = logger;
            _currentSide = Side.Client;
        }


        public IGeneratorId MessageIdGenerator { get; set; } = new TimeGeneratorId();
        protected OperationRuntimeModel Model { get; }

        public IRequestPlaceHolder<TRequest> MessagePlaceHolder { get; set; }

        bool IResponseReceiver<TResponse>.Receive(TResponse result)
        {
            if (_responseQueue.TryRemove(new OperationId(result.Id,result.OperationCode), out var task))
            {
                ((State) task.AsyncState).Result = result;
                task.Start();
                return true;
            }
            return false;
        }

        protected async Task<OperationResult<TResult>> SendOperation<TOp, TResult>(TOp operation, IReadOnlyList<Session> receivers,
            bool forAll, CancellationToken token) where TOp : IOperation<TOp, TResult>
        {
            
            var description = Model.GetDescriptionBy(typeof(TOp));
            
            _logger.Write(LogLevel.Debug,"Start send operation {operation}, code: {code}",operation,description.Code);
            
            var op = new TRequest
            {
                OperationCode = description.Code,
                OperationData = description.UseAsyncSerialize
                    ? await _serializer.SerializeAsync(operation)
                    : _serializer.Serialize(operation),
                Id = MessageIdGenerator.Generate()
            };
            
            MessagePlaceHolder?.Fill(ref op, operation);
            
            _logger.Write(LogLevel.Debug,"Operation serialized {operation}", operation);
            
            var rawResult = _serializer.Serialize(op);
            await SendRequest(receivers, forAll, rawResult, description.ForRequest);
           
            _logger.Write(LogLevel.Debug,"Operation sent {operation}", operation);
            
            if (description.WaitResponse)
            {
                using (var composite = CancellationTokenSource.CreateLinkedTokenSource(token, GlobalToken))
                {
                    Task<OperationResult<TResult>> response = null;
                    try
                    {
                        response = new Task<OperationResult<TResult>>(OperationResultHandle<TResult>, StatesPool.Get(), composite.Token, TaskCreationOptions.PreferFairness);
                        _responseQueue.TryAdd(new OperationId(op.Id, op.OperationCode), response);
                        return await response;
                    }
                    catch (OperationCanceledException)
                    {
                        await SendCancel(receivers, forAll, op);
                        throw;
                    }
                    catch (Exception)
                    {
                        if (response != null)
                        {
                            _responseQueue.TryRemove(new OperationId(op.Id, op.OperationCode), out _);
                            StatesPool.Return((State)response.AsyncState);
                        }
                        throw;
                    }
                }
            }
            return new OperationResult<TResult>(default, BuiltInOperationState.NoWaiting);
            
            
        }

        private OperationResult<TResult> OperationResultHandle<TResult>(object state)
        {
            var s = (State) state;
            var message = s.Result;
            StatesPool.Return(s);

            _logger.Write(LogLevel.Debug, "Receive response {response}", message);
            if (message.OperationData != null && message.Status == BuiltInOperationState.InternalError)
            {
                _logger.Write(LogLevel.Error, "Server error " + _serializer.Deserialize<string>(message.OperationData.To()));
                return new OperationResult<TResult>(default, message.Status);
            }

            if (typeof(TResult) == typeof(Empty)) return new OperationResult<TResult>(default, message.Status);
            return new OperationResult<TResult>(_serializer.Deserialize<TResult>(message.OperationData.To()),
                message.Status);
        }

        private async Task SendCancel(IReadOnlyList<Session> receivers, bool forAll, TRequest request)
        {
            if (_responseQueue.TryRemove(new OperationId(request.Id, request.OperationCode), out var canceledTask))
            {
                StatesPool.Return((State) canceledTask.AsyncState);
                
                await SendRequest(receivers, forAll,
                    _serializer.Serialize(new TRequest
                    {
                        Id = MessageIdGenerator.Generate(),
                        OperationCode = request.OperationCode, 
                        Status = BuiltInOperationState.Cancel
                    }), MinRequiredDeliveryMode.ReliableWithOrdered);
            }
        }

        private async Task SendRequest(IReadOnlyList<Session> receivers, bool forAll, byte[] request, DeliveryMode mode)
        {
            var requestWithMessageType = request.AppendInBegin(TypeMessage.Request);
            switch (_currentSide)
            {
                case Side.Server when forAll:
                    await _sessions.SendToAllAsync(requestWithMessageType,mode);
                    break;
                
                case Side.Server:
                {
                    // ReSharper disable once ForCanBeConvertedToForeach
                    for (int i = 0; i < receivers.Count; i++)
                    {
                        await receivers[i].SendMessageAsync(requestWithMessageType,mode);
                    }
                    break;
                }
                case Side.Client:
                    await _session.SendMessageAsync(requestWithMessageType,mode);
                    break;
            }
        }

        private class State
        {
            public TResponse Result;
        }
       
    }
}