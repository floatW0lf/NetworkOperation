using System.Threading.Tasks;
using NetworkOperation.Host;

namespace NetworkOperation.Server
{
    public interface IHost
    {
        IHostOperationExecutor Executor { get; }
        SessionCollection Sessions { get; }
        void Start(int port);
        void Shutdown();
        Task ShutdownAsync();
    }
}