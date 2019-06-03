using NetworkOperation.Host;

namespace NetworkOperation.Server
{
    public interface IHostContext
    {
        IHostOperationExecutor Executor { get; }
        SessionCollection Sessions { get; }
    }
}