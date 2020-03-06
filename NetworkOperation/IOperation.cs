namespace NetworkOperation
{
    public interface IOperation { }
    public interface IOperation<TOperationResult> : IOperation
    {
    }
}