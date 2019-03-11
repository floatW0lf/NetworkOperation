namespace NetworkOperation
{
    public interface IOperation { }
    public interface IOperation<TSelf,TOperationResult> : IOperation
    {
    }
}