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
        public readonly uint Code;

        public DataWithStateCode(byte[] data, uint code)
        {
            Data = data;
            Code = code;
        }
    }
    
    public abstract class BaseDispatcher<TRequest,TResponse> where TRequest : IOperationMessage, new() where TResponse : IOperationMessage, new()
    {
        private readonly BaseSerializer _serializer;
        private readonly IHandlerFactory _factory;
        protected readonly OperationRuntimeModel Model;
        private readonly IResponsePlaceHolder<TRequest, TResponse> _responsePlaceHolder;
        private IResponseReceiver<TRequest> _responseReceiver;
        private ConcurrentDictionary<uint,CancellationTokenSource> _cancellationMap = new ConcurrentDictionary<uint, CancellationTokenSource>();

        public Action<Exception> ExceptionHandler { get; set; }
        public void Subscribe(IResponseReceiver<TRequest> receiveEvent)
        {
            _responseReceiver = receiveEvent;
        }

        public BaseDispatcher(BaseSerializer serializer, IHandlerFactory factory, OperationRuntimeModel model, IResponsePlaceHolder<TRequest,TResponse> responsePlaceHolder)
        {
            _serializer = serializer;
            _factory = factory;
            Model = model;
            _responsePlaceHolder = responsePlaceHolder;
        }

        public async Task DispatchAsync(Session session)
        {
            if (_responseReceiver == null) throw new Exception("Don't subscribed receive event");
            
            while (session.HasAvailableData)
            {
                var rawMessage = await session.ReceiveMessageAsync();
                var op = _serializer.Deserialize<TRequest>(rawMessage);
                
                if (IsContinue(op)) continue;

                var description = Model.GetDescriptionBy(op.OperationCode);
                try
                {
                    var rawResponse = await ProcessHandler(session, op, description, CreateCancellationToken(op, description));
                    await SendAsync(session, op.OperationCode, rawResponse, op);
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
                            OperationCode = op.OperationCode,
                            StateCode = (uint) BuiltInOperationState.InternalError
                        };
                        await session.SendMessageAsync(_serializer.Serialize(failOp).To());
                    }
                }
                finally
                {
                    RemoveCancellationSource(op);
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
            Console.WriteLine(StatusEncoding.AsString(op.StateCode));
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
        private async Task SendAsync(Session session, uint code, DataWithStateCode rawResponse, TRequest request)
        {
            var sendOp = new TResponse()
            {
                OperationCode = code,
                OperationData = rawResponse.Data,
                StateCode = rawResponse.Code
            };
            _responsePlaceHolder.Fill(ref sendOp, request);
            
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
            

            var result = await typedHandler.Handle(arg, new OperationContext<TRequest>(message, session), token);
            if (operationDescription.UseAsyncSerialize) return new DataWithStateCode(await _serializer.SerializeAsync(result.Result), result.StatusCode);

            return new DataWithStateCode(_serializer.Serialize(result.Result), result.StatusCode);
        }

        protected internal Side ExecutionSide { get; internal set; }
        
    }
}