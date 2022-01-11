using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetworkOperation.Client;
using NetworkOperation.Core;
using NetworkOperation.Core.Dispatching;
using NetworkOperation.Core.Factories;
using NetworkOperation.Core.Messages;

namespace WebGL.WebSockets
{
    public class WebSocketClient<TRequest, TResponse> : AbstractClient<TRequest, TResponse, WebSocket> where TRequest : IOperationMessage, new() where TResponse : IOperationMessage, new()
    {
        private WebSocket _socket;
        private BaseSerializer _serializer;

        public WebSocketClient(IFactory<WebSocket, Session> sessionFactory, IFactory<Session, IClientOperationExecutor> executorFactory, BaseDispatcher<TRequest, TResponse> dispatcher, ILoggerFactory loggerFactory, BaseSerializer serializer) : base(sessionFactory, executorFactory, dispatcher, loggerFactory)
        {
            _serializer = serializer;
        }

        public override Task ConnectAsync(EndPoint remote, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public override Task ConnectAsync<T>(EndPoint remote, T payload, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public override async Task ConnectAsync<T>(Uri connectionUrl, T payload, CancellationToken cancellationToken = new CancellationToken())
        {
            if (connectionUrl.Scheme != "ws") throw new ArgumentException("Web socket support only ws:// scheme.", nameof(connectionUrl));
            if (_socket != null)
            {
                if (_socket.State == WebSocketState.Open || _socket.State == WebSocketState.Connecting) return;
                UnsubscribeEvents();
                _registration.Dispose();
            }
            _registration = cancellationToken.Register(x => ((WebSocket)x).CancelConnection(), _socket);
            _socket = new WebSocket(connectionUrl.ToString(),new Dictionary<string, string>(){{"_payload", Convert.ToBase64String(_serializer.Serialize(payload, null))}});
            SubscribeEvents();
            _socket.Connect();
            await _connect.Task;
        }

        private readonly TaskCompletionSource<int> _connect = new TaskCompletionSource<int>();
        private readonly TaskCompletionSource<int> _disconnect = new TaskCompletionSource<int>();
        private CancellationTokenRegistration _registration;

        private void OnMessage(ArraySegment<byte> data, BufferLifeTime lifeTime)
        {
            ((WebSocketSession)Session).ReceiveMessage(data);
            Dispatch().ContinueWith(_ => lifeTime.Dispose(), TaskContinuationOptions.PreferFairness).GetAwaiter();
        }

        private void OnOpen()
        {
            _connect.SetResult(0);
            OpenSession(_socket);
        }

        private void OnClose(WebSocketCloseCode code)
        {
            _disconnect.SetResult(0);
            CloseSession();
        }

        private void OnRecError(string msg)
        {
            DoErrorSession(new IPEndPoint(IPAddress.Any, 0), SocketError.Shutdown);
        }

        public override async Task DisconnectAsync()
        {
            if(_socket == null || _socket.State == WebSocketState.Closed || _socket.State == WebSocketState.Closing) return;
            _socket.Close();
            await _disconnect.Task;
        }

        public override void Dispose()
        {
            if (_socket != null)
            {
                UnsubscribeEvents();
                _socket.Close();
            }
            _socket = null;
        }

        private void UnsubscribeEvents()
        {
            _socket.OnOpen -= OnOpen;
            _socket.OnError -= OnRecError;
            _socket.OnClose -= OnClose;
            _socket.OnMessage -= OnMessage;
        }

        private void SubscribeEvents()
        {
            _socket.OnOpen += OnOpen;
            _socket.OnError += OnRecError;
            _socket.OnClose += OnClose;
            _socket.OnMessage += OnMessage;
        }
    }

}