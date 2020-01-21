using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetworkOperation.Dispatching;
using NetworkOperation.Extensions;

namespace NetworkOperation
{
    public struct DataWithStateCode
    {
        public readonly byte[] Data;
        public readonly StatusCode Status;

        public DataWithStateCode(byte[] data, StatusCode status)
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

        public BaseDispatcher(BaseSerializer serializer, IHandlerFactory factory, OperationRuntimeModel model, ILoggerFactory logger)
        {
            _serializer = serializer;
            _factory = factory;
            Model = model;
            Logger = logger.CreateLogger(GetType().FullName);
        }

        public async Task DispatchAsync(Session session)
        {
            while (session.HasAvailableData)
            {
                var rawMessage = await session.ReceiveMessageAsync();
                rawMessage = rawMessage.ReadMessageType(out var type);
                
                switch (type)
                {
                    case TypeMessage.Response:
                        if (_responseReceiver == null) throw new Exception("Don't subscribed receive event");
                        _responseReceiver.Receive(_serializer.Deserialize<TResponse>(rawMessage));
                        continue;
                    
                    case TypeMessage.Request:
                        break;
                    
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                var request = _serializer.Deserialize<TRequest>(rawMessage);
                var description = Model.GetDescriptionBy(request.OperationCode);
                try
                {
                    if (GlobalRequestFilter != null)
                    {
                        var response = await GlobalRequestFilter.Handle(new RequestContext<TRequest>(request, session));
                        response.Id = request.Id;
                        if (response.Status != BuiltInOperationState.Success)
                        {
                            await session.SendMessageAsync(_serializer.Serialize(response).AppendInBegin(TypeMessage.Response), description.ForResponse);
                            continue;
                        }
                    }
                    
                    if (IsContinue(request)) continue;
    
                    
                    var rawResponse = await ProcessHandler(session, request, description, CreateCancellationToken(request, description));
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
                            OperationCode = request.OperationCode,
                            Status = BuiltInOperationState.InternalError,
                            OperationData = DebugMode ? _serializer.Serialize(e.Message) : null
                        };
                        await session.SendMessageAsync(_serializer.Serialize(failOp).AppendInBegin(TypeMessage.Response), MinRequiredDeliveryMode.ReliableWithOrdered);
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
                OperationCode = request.OperationCode,
                OperationData = rawResponse.Data,
                Status = rawResponse.Status
            };
            ResponsePlaceHolder?.Fill(ref sendOp, request);
            
            var resultRaw = _serializer.Serialize(sendOp);
            await session.SendMessageAsync(resultRaw.AppendInBegin(TypeMessage.Response), mode);
        }

        protected abstract Task<DataWithStateCode> ProcessHandler(Session session, TRequest message, OperationDescription operationDescription, CancellationToken token);

        protected async Task<DataWithStateCode> GenericHandle<T, TResult>(Session session, TRequest message, OperationDescription operationDescription, CancellationToken token) where T : IOperation<T,TResult>
        {
            var segArray = message.OperationData.To();
            var arg = operationDescription.UseAsyncSerialize
                ? await _serializer.DeserializeAsync<T>(segArray)
                : _serializer.Deserialize<T>(segArray);
            
            // TODO: время жизни обработчика
            var typedHandler = _factory.Create<T,TResult,TRequest>(); 
            var result = await typedHandler.Handle(arg, new RequestContext<TRequest>(message, session), token);
            
            if (!operationDescription.WaitResponse) return default;
            if (typeof(TResult) == typeof(Empty)) return new DataWithStateCode(null, result.Status);
            
            return new DataWithStateCode(operationDescription.UseAsyncSerialize ? await _serializer.SerializeAsync(result.Result) : _serializer.Serialize(result.Result), result.Status);
        }

        protected internal Side ExecutionSide { get; internal set; }
        
    }
}