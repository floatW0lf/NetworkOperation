namespace NetworkOperation.Core
{
    public interface IOperation { }
    public interface IOperation<TOperationResult> : IOperation
    {
    }
}