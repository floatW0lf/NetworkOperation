using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
    public class WebSocketsHttpListenerHost<TRequest,TResponse> : AbstractHost<TRequest,TResponse,WebSocket> where TRequest : IOperationMessage, new() where TResponse : IOperationMessage, new()
    {
        private HttpListener _httpListener;
        private Task _pollAcceptTask;
        private CancellationTokenSource _cts;
        private Task _pollReceive;
        private IEnumerable<Task> _cachedDispatchSessions;


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
        

        public WebSocketsHttpListenerHost(IFactory<SessionCollection, IHostOperationExecutor> executorFactory, BaseDispatcher<TRequest, TResponse> dispatcher, SessionRequestHandler handler, ILoggerFactory loggerFactory) : base(new Factory(), executorFactory, dispatcher, handler, loggerFactory)
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
            _cachedDispatchSessions = Sessions.Select(s => Dispatcher.DispatchAsync(s));
            _pollAcceptTask = Task.Factory.StartNew(PollAccept, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            _pollReceive = Task.Factory.StartNew(PollReceive, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            
            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                _cts?.Cancel();
                _cts?.Dispose();
                _httpListener.Stop();
                CloseAllSession();
                return Task.WhenAll(_pollReceive, _pollAcceptTask);
            }
            finally
            {
                _cts = null;
                _httpListener = null;
            }
           
        }

        private readonly ConcurrentQueue<Session> _openingSessionQueue = new ConcurrentQueue<Session>();

        private async Task PollAccept()
        {
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    _cts.Token.ThrowIfCancellationRequested();
                    var context = await _httpListener.GetContextAsync();
                    if (context.Request.IsWebSocketRequest)
                    {
                        _cts.Token.ThrowIfCancellationRequested();
                        var webSocketContext = await context.AcceptWebSocketAsync(SubProtocol);
                        var request = new HttpListenerWebSocketsRequest(webSocketContext, _openingSessionQueue, context.Request.RemoteEndPoint);
                        BeforeSessionOpen(request);
                        await Task.Delay(PollTimeInMs, _cts.Token);
                    }
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Accept connection error");
                }
               
            }
        }

        private async Task PollReceive()
        {
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    while (_openingSessionQueue.TryDequeue(out var session))
                    {
                        _cts.Token.ThrowIfCancellationRequested();
                        SessionOpen(session);
                    }
            
                    foreach (var session in Sessions)
                    {
                        if (session.State == SessionState.Closed) SessionClose(session);
                    }
            
                    await Task.WhenAll(_cachedDispatchSessions);
                    await Task.Delay(PollTimeInMs, _cts.Token);
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Event loop error");
                }
                
            }
        }
    }
}