using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetworkOperation.Core.Messages;
using NetworkOperation.Core.Models;

namespace NetworkOperation.Core.Dispatching
{
    public struct DataWithStateCode
    {
        public readonly ReadOnlyMemory<byte> Data;
        public readonly StatusCode Status;

        public DataWithStateCode(ReadOnlyMemory<byte> data, StatusCode status)
        {
            Data = data;
            Status = status;
        }
    }
    
    public abstract class BaseDispatcher<TRequest,TResponse> where TRequest : IOperationMessage, new() where TResponse : IOperationMessage, new()
    {
        private readonly BaseSerializer _serializer;
        private readonly IHandlerFactory _factory;
        protected readonly OperationRuntimeModel Model;
        private readonly DescriptionRuntimeModel _descriptionRuntimeModel;
        protected ILogger Logger { get; }
        
        private IResponseReceiver<TResponse> _responseReceiver;
        
        private ConcurrentDictionary<uint,CancellationTokenSource> _cancellationMap = new ConcurrentDictionary<uint, CancellationTokenSource>();

        public bool DebugMode { get; set; }
        public IResponsePlaceHolder<TRequest, TResponse> ResponsePlaceHolder { get; set; }
        public IRequestFilter<TRequest,TResponse> GlobalRequestFilter { get; set; }
        public void Subscribe(IResponseReceiver<TResponse> receiveEvent)
        {
            _responseReceiver = receiveEvent;
        }

        public BaseDispatcher(BaseSerializer serializer, IHandlerFactory factory, OperationRuntimeModel model, ILoggerFactory logger, DescriptionRuntimeModel descriptionRuntimeModel)
        {
            _serializer = serializer;
            _factory = factory;
            Model = model;
            _descriptionRuntimeModel = descriptionRuntimeModel;
            Logger = logger.CreateLogger(GetType().FullName);
        }

        public async Task DispatchAsync(Session session)
        {
            while (session.HasAvailableData)
            {
                var rawMessage = await session.ReceiveMessageAsync();
                var type = _serializer.ReadMessageType(rawMessage);
                switch (type)
                {
                    case TypeMessage.Response:
                        if (_responseReceiver == null) throw new Exception("Don't subscribed receive event");
                        _responseReceiver.Receive(_serializer.Deserialize<TResponse>(rawMessage,session));
                        continue;
                    
                    case TypeMessage.Request:
                        break;
                    
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                var request = _serializer.Deserialize<TRequest>(rawMessage,session);
                var description = Model.GetDescriptionBy(request.OperationCode);
                var context = new RequestContext<TRequest>(request, session, description,_descriptionRuntimeModel.GetByOperation(description.OperationType));
                try
                {
                    if (GlobalRequestFilter != null)
                    {
                        var response = await GlobalRequestFilter.Handle(context);
                        response.Id = request.Id;
                        response.Type = TypeMessage.Response;
                        if (response.Status != BuiltInOperationState.Success)
                        {
                            await session.SendMessageAsync(_serializer.Serialize(response, session), description.ForResponse);
                            continue;
                        }
                    }
                    
                    if (IsContinue(request)) continue;
    
                    
                    var rawResponse = await ProcessHandler(request, context, CreateCancellationToken(request, description));
                    if (description.WaitResponse)
                    {
                        await SendAsync(session, rawResponse, request, description.ForResponse);
                    }
                    
                }
                catch (OperationCanceledException e) { Logger.LogInformation("Operation canceled: {request}, {exception}", request, e); }
                catch (Exception e)
                {
                    try
                    {
                        Logger.LogError("Handle error : {exception}", e);
                    }
                    finally
                    {
                        var failOp = new TResponse()
                        {
                            Id = request.Id,
                            Type = TypeMessage.Response,
                            OperationCode = request.OperationCode,
                            Status = BuiltInOperationState.InternalError,
                            OperationData = DebugMode ? _serializer.Serialize(e.Message,session) : default
                        };
                        await session.SendMessageAsync(_serializer.Serialize(failOp,session), MinRequiredDeliveryMode.ReliableWithOrdered);
                    }
                }
                finally
                {
                    RemoveCancellationSource(request);
                }
            }
        }

        private bool IsContinue(TRequest op)
        {
            return TryOperationCancel(op);
        }

        private CancellationToken CreateCancellationToken(TRequest op, OperationDescription description)
        {
            var cts = _cancellationMap.GetOrAdd(op.OperationCode, u => new CancellationTokenSource());
            return cts.Token;
        }

        private bool RemoveAndGetCts(TRequest op, out CancellationTokenSource cancellationTokenSource)
        {
            cancellationTokenSource = null; 
            return op.Status == BuiltInOperationState.Cancel &&
                   _cancellationMap.TryRemove(op.OperationCode, out cancellationTokenSource);
        }
        private bool TryOperationCancel(TRequest op)
        {
            if (RemoveAndGetCts(op, out var cts))
            {
                cts.Cancel();
                cts.Dispose();
                return true;
            }
            return false;
        }

        
        private void RemoveCancellationSource(TRequest op)
        {
            if (RemoveAndGetCts(op,out var cts))
            {
                cts.Dispose();
            }
        }
        
        private async Task SendAsync(Session session, DataWithStateCode rawResponse, TRequest request, DeliveryMode mode)
        {
            var sendOp = new TResponse
            {
                Id = request.Id,
                Type = TypeMessage.Response,
                OperationCode = request.OperationCode,
                OperationData = rawResponse.Data,
                Status = rawResponse.Status
            };
            ResponsePlaceHolder?.Fill(ref sendOp, request);
            
            var resultRaw = _serializer.Serialize(sendOp,session);
            await session.SendMessageAsync(resultRaw, mode);
        }

        protected abstract Task<DataWithStateCode> ProcessHandler(TRequest header, RequestContext<TRequest> context, CancellationToken token);

        protected async Task<DataWithStateCode> GenericHandle<T, TResult>(TRequest header, RequestContext<TRequest> context, CancellationToken token) where T : IOperation<TResult>
        {
            var segArray = header.OperationData;
            var arg = context.OperationDescription.UseAsyncSerialize
                ? await _serializer.DeserializeAsync<T>(segArray,context.Session)
                : _serializer.Deserialize<T>(segArray,context.Session);
            
            var typedHandler = _factory.Create<T,TResult,TRequest>(context);
            var result = await typedHandler.Handle(arg, context, token);
            
            if (context.HandlerDescription.LifeTime == Scope.Request)
            {
                _factory.Destroy(typedHandler);
            }
            
            if (!context.OperationDescription.WaitResponse) return default;
            if (typeof(TResult) == typeof(Empty)) return new DataWithStateCode(null, result.Status);
            
            return new DataWithStateCode(context.OperationDescription.UseAsyncSerialize ? await _serializer.SerializeAsync(result.Result,context.Session) : _serializer.Serialize(result.Result,context.Session), result.Status);
        }

        protected internal Side ExecutionSide { get; internal set; }
        
    }
}