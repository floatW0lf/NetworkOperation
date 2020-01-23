namespace NetworkOperation.Host
{
    public interface IHostContext
    {
        IHostOperationExecutor Executor { get; }
        SessionCollection Sessions { get; }
    }
}