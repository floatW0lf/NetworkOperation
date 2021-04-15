using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using NetworkOperation.Core.Messages;
using NetworkOperation.Core.Models;

namespace NetworkOperation.Core
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
        protected ILogger Logger { get; }

        public CancellationToken GlobalToken { get; set; }
        
        protected BaseOperationExecutor(OperationRuntimeModel model, BaseSerializer serializer, ILoggerFactory loggerFactory)
        {
            Model = model;
            _serializer = serializer;
            Logger = loggerFactory.CreateLogger(GetType().FullName);
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

        protected async Task<OperationResult<TResult>> SendOperation<TOp, TResult>(TOp operation, IEnumerable<Session> receivers, CancellationToken token) where TOp : IOperation<TResult>
        {
            
            var description = Model.GetDescriptionBy(typeof(TOp));
            
            Logger.LogDebug("Start send operation {operation}, code: {code}",operation,description.Code);
            
            var op = new TRequest
            {
                OperationCode = description.Code,
                OperationData = description.UseAsyncSerialize
                    ? await _serializer.SerializeAsync(operation, null)
                    : _serializer.Serialize(operation, null),
                Id = MessageIdGenerator.Generate()
            };
            
            MessagePlaceHolder?.Fill(ref op, operation);
            
            Logger.LogDebug("Operation serialized {operation}", operation);
            
            var rawResult = _serializer.Serialize(op, null);
            await SendRequest(receivers, rawResult, description.ForRequest);
           
            Logger.LogDebug("Operation sent {operation}", operation);
            
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
                        await SendCancel(receivers, op);
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

            Logger.LogDebug("Receive response {response}", message);
            if (message.Status == BuiltInOperationState.InternalError)
            {
                Logger.LogError("Server error: " + (message.OperationData != null ? _serializer.Deserialize<string>(message.OperationData.To(), null) : "empty message because on server off debug mode "));
                return new OperationResult<TResult>(default, message.Status);
            }

            if (typeof(TResult) == typeof(Empty)) return new OperationResult<TResult>(default, message.Status);
            return new OperationResult<TResult>(_serializer.Deserialize<TResult>(message.OperationData.To(), null), message.Status);
        }

        private async Task SendCancel(IEnumerable<Session> receivers, TRequest request)
        {
            if (_responseQueue.TryRemove(new OperationId(request.Id, request.OperationCode), out var canceledTask))
            {
                StatesPool.Return((State) canceledTask.AsyncState);
                
                await SendRequest(receivers,
                    _serializer.Serialize(new TRequest
                    {
                        Id = MessageIdGenerator.Generate(),
                        OperationCode = request.OperationCode, 
                        Status = BuiltInOperationState.Cancel
                    }, null), MinRequiredDeliveryMode.ReliableWithOrdered);
            }
        }

        protected abstract Task SendRequest(IEnumerable<Session> receivers, byte[] request, DeliveryMode mode);

        private class State
        {
            public TResponse Result;
        }
       
    }
}