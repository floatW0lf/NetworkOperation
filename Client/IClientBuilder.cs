namespace NetworkOperation.Client
{
    public interface IClientBuilder<TRequest,TResponse> 
        where TRequest : IOperationMessage, new() 
        where TResponse : IOperationMessage, new()
    {
        IHandlerFactory HandlerFactory { get; set; }
        BaseSerializer Serializer { get; set; }
        OperationRuntimeModel Model { get; set; }
        
        void RegisterHandler<TOperation, TResult>() where TOperation : IOperation<TOperation,TResult>;
        void RegisterHandler<TOperation, TResult>(IHandler<TOperation,TResult,TRequest> handler) where TOperation : IOperation<TOperation,TResult>;
        
        IClient Build();
    }
}