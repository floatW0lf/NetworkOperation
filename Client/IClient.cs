using System.Threading.Tasks;

namespace NetworkOperation.Client
{
    public interface IClient
    {
        IClientOperationExecutor Executor { get; }
        void Connect(string address, int port);
        Task ConnectAsync(string address, int port);
        void Disconnect();
        Task DisconnectAsync();
    }
}