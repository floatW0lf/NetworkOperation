using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetworkOperation.Client;
using NetworkOperation.Core;
using NetworkOperation.Core.Dispatching;
using NetworkOperation.Core.Factories;
using NetworkOperation.Core.Messages;

namespace NetworkOperation.WebSockets.Client
{
    public class WebSocketClient<TRequest,TResponse> : AbstractClient<TRequest,TResponse, WebSocket> where TRequest : IOperationMessage, new() where TResponse : IOperationMessage, new()
    {
        public WebSocketClient(IFactory<WebSocket, Session> sessionFactory, IFactory<Session, IClientOperationExecutor> executorFactory, BaseDispatcher<TRequest, TResponse> dispatcher, ILoggerFactory loggerFactory) : base(sessionFactory, executorFactory, dispatcher, loggerFactory)
        {
            
        }

        public override async Task ConnectAsync(EndPoint remote, CancellationToken cancellationToken = default)
        {
            
        }

        public override async Task ConnectAsync<T>(EndPoint remote, T payload, CancellationToken cancellationToken = default)
        {
            
        }

        public override async Task DisconnectAsync()
        {
            
        }

        public override void Dispose()
        {
            
        }
    }
}

