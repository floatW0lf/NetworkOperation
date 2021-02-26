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
        
        private readonly ConcurrentDictionary<OperationId, (object source, Action<TResponse,object> converter)> _responseQueue = new ConcurrentDictionary<OperationId, (object, Action<TResponse,object>)>();
        private readonly BaseSerializer _serializer;
        
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
            if (_responseQueue.TryRemove(new OperationId(result.Id,result.OperationCode), out var completeSource))
            {
                completeSource.converter(result, completeSource.source);
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
                    ? await _serializer.SerializeAsync(operation)
                    : _serializer.Serialize(operation),
                Id = MessageIdGenerator.Generate()
            };
            
            MessagePlaceHolder?.Fill(ref op, operation);
            
            Logger.LogDebug("Operation serialized {operation}", operation);
            
            var rawResult = _serializer.Serialize(op);
            await SendRequest(receivers, rawResult, description.ForRequest);
           
            Logger.LogDebug("Operation sent {operation}", operation);

            using (var cancellation = CancellationTokenSource.CreateLinkedTokenSource(token, GlobalToken))
            {
                if (description.WaitResponse)
                {
                    try
                    {
                        var source = new TaskCompletionSource<OperationResult<TResult>>();
                        _responseQueue.TryAdd(new OperationId(op.Id, op.OperationCode), (source, GenericHandle<TResult>));
                        using (cancellation.Token.Register(() => { source.SetCanceled(); }))
                        {
                            return await source.Task;
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        await SendCancel(receivers, op);
                        throw;
                    }
                    catch (Exception)
                    {
                        _responseQueue.TryRemove(new OperationId(op.Id, op.OperationCode), out _);
                        throw;
                    }
                }
                return new OperationResult<TResult>(default, BuiltInOperationState.NoWaiting);
            }
            
        }

        private void GenericHandle<TResult>(TResponse response, object source)
        {
            var taskCompletionSource = (TaskCompletionSource<OperationResult<TResult>>) source;
            taskCompletionSource.SetResult(OperationResultHandle<TResult>(response));
        }

        private OperationResult<TResult> OperationResultHandle<TResult>(TResponse message)
        {
            Logger.LogDebug("Receive response {response}", message);
            if (message.Status == BuiltInOperationState.InternalError)
            {
                Logger.LogError("Server error: " + (message.OperationData != null ? _serializer.Deserialize<string>(message.OperationData.To()) : "empty message because on server off debug mode "));
                return new OperationResult<TResult>(default, message.Status);
            }

            if (typeof(TResult) == typeof(Empty)) return new OperationResult<TResult>(default, message.Status);
            return new OperationResult<TResult>(_serializer.Deserialize<TResult>(message.OperationData.To()), message.Status);
        }

        private async Task SendCancel(IEnumerable<Session> receivers, TRequest request)
        {
            if (_responseQueue.TryRemove(new OperationId(request.Id, request.OperationCode), out var canceledTask))
            {
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

    }
}