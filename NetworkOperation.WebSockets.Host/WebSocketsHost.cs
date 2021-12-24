using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetworkOperation.Core;
using NetworkOperation.Core.Dispatching;
using NetworkOperation.Core.Factories;
using NetworkOperation.Core.Messages;
using NetworkOperation.Host;

namespace NetworkOperation.WebSockets.Host
{
    public class WebSocketsHost<TRequest,TResponse> : AbstractHost<TRequest,TResponse,WebSocket> where TRequest : IOperationMessage, new() where TResponse : IOperationMessage, new()
    {
        private HttpListener _httpListener;
        private Task _pollAcceptTask;
        private CancellationTokenSource _cts;
        private Task _pollReceive;


        public string UriHost { get; set; }
        public string SubProtocol { get; set; }
        
        private class Factory : IFactory<WebSocket, MutableSessionCollection>
        {
            class WebSocketCollection : MutableSessionCollection
            {
                public override NetworkStatistics Statistics => throw new NotImplementedException();
            }
            public MutableSessionCollection Create(WebSocket arg)
            {
                return new WebSocketCollection();
            }
        }
        

        public WebSocketsHost(IFactory<SessionCollection, IHostOperationExecutor> executorFactory, BaseDispatcher<TRequest, TResponse> dispatcher, SessionRequestHandler handler, ILoggerFactory loggerFactory) : base(new Factory(), executorFactory, dispatcher, handler, loggerFactory)
        {
            
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add(UriHost);
            _httpListener.Start();
            ServerStarted(null);
            _pollAcceptTask = Task.Factory.StartNew(PollAccept, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default).Unwrap();
            _pollReceive = Task.Factory.StartNew(PollReceive, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default).Unwrap();
            
            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _cts?.Dispose();
            _cts = null;
            _httpListener.Stop();
            _httpListener = null;
            CloseAllSession();
            return Task.WhenAll(_pollReceive, _pollAcceptTask);
        }


        private async Task PollAccept()
        {
            _cts.Token.ThrowIfCancellationRequested();
            var context = await _httpListener.GetContextAsync();
            if (context.Request.IsWebSocketRequest)
            {
                _cts.Token.ThrowIfCancellationRequested();
                var webSocketContext = await context.AcceptWebSocketAsync(SubProtocol);
                BeforeSessionOpen(new WebSocketsRequest(webSocketContext));
                await Task.Delay(PollTimeInMs, _cts.Token);
            }
        }

        private async Task PollReceive()
        {
            foreach (var session in Sessions)
            {
                switch (session.State)
                {
                    case SessionState.Opened:
                        await Dispatcher.DispatchAsync(session);
                        break;
                    case SessionState.Closed:
                        SessionClose(session);
                        break;
                }
            }
            await Task.Delay(PollTimeInMs, _cts.Token);
        }
    }
}