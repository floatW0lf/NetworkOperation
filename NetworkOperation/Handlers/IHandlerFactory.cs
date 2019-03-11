namespace NetworkOperation
{
    public interface IHandlerFactory
    {
        IHandler<TOperation,TResult,TRequest> Create<TOperation,TResult,TRequest>() where TOperation : IOperation<TOperation, TResult> where TRequest : IOperationMessage;
        void Destroy(IHandler handler);
    }
}