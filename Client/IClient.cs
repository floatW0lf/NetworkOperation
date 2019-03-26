using System.Threading.Tasks;

namespace NetworkOperation.Client
{
    public enum ClientState
    {
        Disconnected,
        Connected,
        Connecting
    }
    public interface IClient
    {
        ClientState Current { get; }
        IClientOperationExecutor Executor { get; }
        void Connect(string address, int port);
        Task ConnectAsync(string address, int port);
        void Disconnect();
        Task DisconnectAsync();
    }
}