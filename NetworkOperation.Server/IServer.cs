using System.Threading.Tasks;

namespace NetworkOperation.Server
{
    public interface IServer
    {
        IServerOperationExecutor Executor { get; }
        SessionCollection Sessions { get; }
        void Start(int port);
        void Shutdown();
        Task ShutdownAsync();
    }
}