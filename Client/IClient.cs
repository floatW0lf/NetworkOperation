using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkOperation.Client
{
    public enum ClientState
    {
        Disconnected,
        Connected,
        Connecting
    }
    public interface IClient : IDisposable
    {
        ClientState Current { get; }
        IClientOperationExecutor Executor { get; }
        Task ConnectAsync(EndPoint remote, CancellationToken cancellationToken = default);
        Task DisconnectAsync();
    }
}