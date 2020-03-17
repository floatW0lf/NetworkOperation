using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using NetworkOperation.Core;

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
        IPayloadResolver ConnectionPayload { get; set; }
        
        Task ConnectAsync(EndPoint remote, CancellationToken cancellationToken = default);
        Task ConnectAsync<T>(EndPoint remote, T payload, CancellationToken cancellationToken = default) where T : IConnectPayload;
        Task DisconnectAsync();
    }
}