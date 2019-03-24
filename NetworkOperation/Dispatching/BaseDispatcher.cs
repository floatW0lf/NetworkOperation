using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using NetworkOperation.Extensions;
using NetworkOperation.StatusCodes;

namespace NetworkOperation
{
    public struct DataWithStateCode
    {
        public readonly byte[] Data;
        public readonly uint StatusCode;

        public DataWithStateCode(byte[] data, uint statusCode)
        {
            Data = data;
            StatusCode = statusCode;
        }
    }
    
    public abstract class BaseDispatcher<TRequest,TResponse> where TRequest : IOperationMessage, new() where TResponse : IOperationMessage, new()
    {
        private readonly BaseSerializer _serializer;
        private readonly IHandlerFactory _factory;
        protected readonly OperationRuntimeModel Model;
        private IResponseReceiver<TRequest> _responseReceiver;
        
        private ConcurrentDictionary<uint,CancellationTokenSource> _cancellationMap = new ConcurrentDictionary<uint, CancellationTokenSource>();

        public bool DebugMode { get; set; }
        public Action<Exception> ExceptionHandler { get; set; }
        public IResponsePlaceHolder<TRequest, TResponse> ResponsePlaceHolder { get; set; }
        public IRequestFilter<TRequest,TResponse> GlobalRequestFilter { get; set; }
        public void Subscribe(IResponseReceiver<TRequest> receiveEvent)
        {
            _responseReceiver = receiveEvent;
        }

        public BaseDispatcher(BaseSerializer serializer, IHandlerFactory factory, OperationRuntimeModel model)
        {
            _serializer = serializer;
            _factory = factory;
            Model = model;
        }

        public async Task DispatchAsync(Session session)
        {
            if (_responseReceiver == null) throw new Exception("Don't subscribed receive event");
            
            while (session.HasAvailableData)
            {
                var rawMessage = await session.ReceiveMessageAsync();
                var request = _serializer.Deserialize<TRequest>(rawMessage);
                try
                {
                    if (GlobalRequestFilter != null)
                    {
                        var response = await GlobalRequestFilter.Handle(new RequestContext<TRequest>(request, session));
                        if (response.StateCode != (uint)BuiltInOperationState.Success)
                        {
                            await session.SendMessageAsync(_serializer.Serialize(response).To());
                            continue;
                        }
                    }
                    
                    if (IsContinue(request)) continue;
    
                    var description = Model.GetDescriptionBy(request.OperationCode);
                    var rawResponse = await ProcessHandler(session, request, description, CreateCancellationToken(request, description));
                    if (description.WaitResponse)
                    {
                        await SendAsync(session, rawResponse, request);
                    }
                    
                }
                catch (OperationCanceledException e) { ExceptionHandler?.Invoke(e); }
                catch (Exception e)
                {
                    try
                    {
                        ExceptionHandler?.Invoke(e);
                    }
                    finally
                    {
                        var failOp = new TResponse()
                        {
                            Id = request.Id,
                            OperationCode = request.OperationCode,
                            StateCode = (uint) BuiltInOperationState.InternalError,
                            OperationData = DebugMode ? _serializer.Serialize(e.Message) : null
                        };
                        await session.SendMessageAsync(_serializer.Serialize(failOp).To());
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
            return TryOperationCancel(op) || _responseReceiver.Receive(op);
        }

        private CancellationToken CreateCancellationToken(TRequest op, OperationDescription description)
        {
            var cts = _cancellationMap.GetOrAdd(op.OperationCode, u => new CancellationTokenSource());
            return cts.Token;
        }

        private bool TryOperationCancel(TRequest op)
        {
            if (op.StateCode == (uint)BuiltInOperationState.Cancel)
            {
                if (_cancellationMap.TryRemove(op.OperationCode, out var cts))
                {
                    cts.Cancel();
                    cts.Dispose();
                }
                
                return true;
            }
            return false;
        }

        private void RemoveCancellationSource(TRequest op)
        {
            if (op.StateCode == (uint)BuiltInOperationState.Cancel && _cancellationMap.TryRemove(op.OperationCode, out var cts))
            {
                cts.Dispose();
            }
        }
        
        private async Task SendAsync(Session session, DataWithStateCode rawResponse, TRequest request)
        {
            var sendOp = new TResponse
            {
                Id = request.Id,
                OperationCode = request.OperationCode,
                OperationData = rawResponse.Data,
                StateCode = rawResponse.StatusCode
            };
            ResponsePlaceHolder?.Fill(ref sendOp, request);
            
            var resultRaw = _serializer.Serialize(sendOp);
            await session.SendMessageAsync(resultRaw.To());
        }

        protected abstract Task<DataWithStateCode> ProcessHandler(Session session, TRequest message, OperationDescription operationDescription, CancellationToken token);

        protected async Task<DataWithStateCode> GenericHandle<T, TResult>(Session session, TRequest message, OperationDescription operationDescription, CancellationToken token) where T : IOperation<T,TResult>
        {
            // TODO: время жизни обработчика
            var typedHandler = _factory.Create<T,TResult,TRequest>(); 
            var segArray = message.OperationData.To();
            var arg = operationDescription.UseAsyncSerialize
                ? await _serializer.DeserializeAsync<T>(segArray)
                : _serializer.Deserialize<T>(segArray);
            

            var result = await typedHandler.Handle(arg, new RequestContext<TRequest>(message, session), token);
            if (operationDescription.UseAsyncSerialize) return new DataWithStateCode(await _serializer.SerializeAsync(result.Result), result.StatusCode);

            return new DataWithStateCode(_serializer.Serialize(result.Result), result.StatusCode);
        }

        protected internal Side ExecutionSide { get; internal set; }
        
    }
}