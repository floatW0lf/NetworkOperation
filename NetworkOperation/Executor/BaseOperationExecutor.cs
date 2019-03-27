using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NetworkOperation.Dispatching;
using NetworkOperation.Extensions;

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

        private readonly Pool<State> _states = new Pool<State>(() => new State(), 10, 100);
        private readonly Side _currentSide;
        private readonly Session _session;
        private readonly SessionCollection _sessions;
        

        public CancellationToken GlobalToken { get; set; }
        
        protected BaseOperationExecutor(OperationRuntimeModel model, BaseSerializer serializer, SessionCollection sessions)
        {
            Model = model;
            _serializer = serializer;
            _sessions = sessions;
            _currentSide = Side.Server;
        }

        protected BaseOperationExecutor(OperationRuntimeModel model, BaseSerializer serializer, Session session)
        {
            
            Model = model;
            _serializer = serializer;
            _session = session;
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
            var op = new TRequest
            {
                OperationCode = description.Code,
                OperationData = description.UseAsyncSerialize
                    ? await _serializer.SerializeAsync(operation)
                    : _serializer.Serialize(operation),
                Id = MessageIdGenerator.Generate()
            };
            
            MessagePlaceHolder?.Fill(ref op, operation);

            var rawResult = _serializer.Serialize(op);
            await SendRawOperation(receivers, forAll, rawResult);
           
            try
            {
                if (description.WaitResponse)
                {
                    using (var composite = CancellationTokenSource.CreateLinkedTokenSource(token, GlobalToken))
                    {
                        Task<OperationResult<TResult>> response = null;
                        try 
                        {
                            response = new Task<OperationResult<TResult>>(state =>
                            {
                                var s = (State) state;
                                var message = s.Result;
                                _states.Put(s);
                            
                                if (typeof(TResult) == typeof(Empty)) return new OperationResult<TResult>(default, message.StatusCode);
                                return new OperationResult<TResult>(_serializer.Deserialize<TResult>(message.OperationData.To()), message.StatusCode);
                            
                            }, _states.Rent(), composite.Token, TaskCreationOptions.PreferFairness);
                            _responseQueue.TryAdd(new OperationId(op.Id, op.OperationCode), response);
                            return await response;
                        }
                        catch (Exception e)
                        {
                            _states.Put((State)response.AsyncState);
                            throw;
                        }
                    }
                }
                return new OperationResult<TResult>(default, (uint)BuiltInOperationState.NoWaiting);
            }
            catch (OperationCanceledException)
            {
                await SendCancel(receivers, forAll, op);
                throw;
            }
        }

        private async Task SendCancel(IReadOnlyList<Session> receivers, bool forAll, TRequest request)
        {
            if (_responseQueue.TryRemove(new OperationId(request.Id, request.OperationCode), out var canceledTask))
            {
                _states.Put((State) canceledTask.AsyncState);
                
                await SendRawOperation(receivers, forAll,
                    _serializer.Serialize(new TRequest
                    {
                        Id = MessageIdGenerator.Generate(),
                        OperationCode = request.OperationCode, 
                        StatusCode = (uint)BuiltInOperationState.Cancel
                    }));
            }
        }

        private async Task SendRawOperation(IReadOnlyList<Session> receivers, bool forAll, byte[] request)
        {
            switch (_currentSide)
            {
                case Side.Server when forAll:
                    await _sessions.SendToAllAsync(request.AppendInBegin(TypeMessage.Request));
                    break;
                
                case Side.Server:
                {
                    var rawWithMessageType = request.AppendInBegin(TypeMessage.Request);
                    foreach (var r in receivers)
                    {
                        await r.SendMessageAsync(rawWithMessageType);
                    }
                    break;
                }
                case Side.Client:
                    await _session.SendMessageAsync(request.AppendInBegin(TypeMessage.Request));
                    break;
            }
        }

        private class State
        {
            public TResponse Result;
        }
       
    }
}