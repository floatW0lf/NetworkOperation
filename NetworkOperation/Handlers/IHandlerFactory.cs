namespace NetworkOperation
{
    public interface IHandlerFactory
    {
        IHandler<TOperation,TResult,TRequest> Create<TOperation,TResult,TRequest>(RequestContext<TRequest> requestContext) where TOperation : IOperation<TOperation, TResult> where TRequest : IOperationMessage;
        void Destroy(IHandler handler);
    }
}