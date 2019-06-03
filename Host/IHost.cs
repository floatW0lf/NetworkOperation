using System.Threading.Tasks;
using NetworkOperation.Host;

namespace NetworkOperation.Server
{
    public interface IHostOperation
    {
        IHostOperationExecutor Executor { get; }
        SessionCollection Sessions { get; }
    }
    public interface IHost : IHostOperation
    {
        void Start(int port);
        void Shutdown();
        Task ShutdownAsync();
    }
}