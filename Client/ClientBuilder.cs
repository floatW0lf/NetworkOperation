namespace NetworkOperation.Client
{
    public class ClientBuilder<TRequest,TResponse> : IClientBuilder<TRequest,TResponse> where TRequest : IOperationMessage, new() where TResponse : IOperationMessage, new()
    {
        public IHandlerFactory HandlerFactory { get; set; }
        public BaseSerializer Serializer { get; set; }
        public OperationRuntimeModel Model { get; set; }
        
        public void RegisterHandler<TOperation, TResult>() where TOperation : IOperation<TOperation, TResult>
        {
            
        }

        public void RegisterHandler<TOperation, TResult>(IHandler<TOperation, TResult, TRequest> handler) where TOperation : IOperation<TOperation, TResult>
        {
            
        }

        public IClient Build()
        {
            
        }
    }
}