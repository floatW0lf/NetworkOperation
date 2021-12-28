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
        
        [Obsolete("Use ConnectAsync<T>(Uri connectionUrl, T payload, CancellationToken cancellationToken)")]
        Task ConnectAsync(EndPoint remote, CancellationToken cancellationToken = default);
        [Obsolete("Use ConnectAsync<T>(Uri connectionUrl, T payload, CancellationToken cancellationToken)")]
        Task ConnectAsync<T>(EndPoint remote, T payload, CancellationToken cancellationToken = default) where T : IConnectPayload;
        Task ConnectAsync<T>(Uri connectionUrl, T payload, CancellationToken cancellationToken = default) where T : IConnectPayload;
        Task DisconnectAsync();
    }
}