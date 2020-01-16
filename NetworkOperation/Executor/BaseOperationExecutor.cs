﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
        protected IStructuralLogger Logger { get; }

        public CancellationToken GlobalToken { get; set; }
        
        protected BaseOperationExecutor(OperationRuntimeModel model, BaseSerializer serializer,IStructuralLogger logger)
        {
            Model = model;
            _serializer = serializer;
            Logger = logger;
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

        protected async Task<OperationResult<TResult>> SendOperation<TOp, TResult>(TOp operation, IEnumerable<Session> receivers, CancellationToken token) where TOp : IOperation<TOp, TResult>
        {
            
            var description = Model.GetDescriptionBy(typeof(TOp));
            
            Logger.Write(LogLevel.Debug,"Start send operation {operation}, code: {code}",operation,description.Code);
            
            var op = new TRequest
            {
                OperationCode = description.Code,
                OperationData = description.UseAsyncSerialize
                    ? await _serializer.SerializeAsync(operation)
                    : _serializer.Serialize(operation),
                Id = MessageIdGenerator.Generate()
            };
            
            MessagePlaceHolder?.Fill(ref op, operation);
            
            Logger.Write(LogLevel.Debug,"Operation serialized {operation}", operation);
            
            var rawResult = _serializer.Serialize(op);
            await SendRequest(receivers, rawResult, description.ForRequest);
           
            Logger.Write(LogLevel.Debug,"Operation sent {operation}", operation);
            
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

            Logger.Write(LogLevel.Debug, "Receive response {response}", message);
            if (message.OperationData != null && message.Status == BuiltInOperationState.InternalError)
            {
                Logger.Write(LogLevel.Error, "Server error " + _serializer.Deserialize<string>(message.OperationData.To()));
                return new OperationResult<TResult>(default, message.Status);
            }

            if (typeof(TResult) == typeof(Empty)) return new OperationResult<TResult>(default, message.Status);
            return new OperationResult<TResult>(_serializer.Deserialize<TResult>(message.OperationData.To()),
                message.Status);
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
                    }), MinRequiredDeliveryMode.ReliableWithOrdered);
            }
        }

        protected abstract Task SendRequest(IEnumerable<Session> receivers, byte[] request, DeliveryMode mode);

        private class State
        {
            public TResponse Result;
        }
       
    }
}