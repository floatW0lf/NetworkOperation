using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NetworkOperation.Core;
using NetworkOperation.Core.Dispatching;
using NetworkOperation.Core.Factories;
using NetworkOperation.Core.Messages;
using NetworkOperation.Host;
using NetworkOperation.WebSockets.Core;

namespace NetworkOperation.WebSockets.Host
{
    public class WebSocketsAspNetHost<TRequest,TResponse> : AbstractHost<TRequest,TResponse, NoneConnectionCollection>, IMiddleware where TRequest : IOperationMessage, new() where TResponse : IOperationMessage, new()
    {
        private Task _checkTask;
        private Task _dispatchTask;
        
        public string SubProtocol { get; set; }
        public string ConnectionPath { get; set; }
        
        public TimeSpan TimeOutConnection { get; set; } = TimeSpan.FromSeconds(2);
        public WebSocketsAspNetHost(IFactory<NoneConnectionCollection, MutableSessionCollection> sessionsFactory, IFactory<SessionCollection, IHostOperationExecutor> executorFactory, BaseDispatcher<TRequest, TResponse> dispatcher, SessionRequestHandler handler, ILoggerFactory loggerFactory) : base(sessionsFactory, executorFactory, dispatcher, handler, loggerFactory)
        {
        }
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _checkTask = Task.Factory.StartNew(CheckClosedSession, cancellationToken,TaskCreationOptions.LongRunning, TaskScheduler.Default);
            _dispatchTask = Task.Factory.StartNew(DispatchSessions, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            _dispatchCollection = Sessions.Select(s => Dispatcher.DispatchAsync(s));
            _cancellationTokenSource = new CancellationTokenSource();
            ServerStarted(default);
            return Task.CompletedTask;
        }

        private async void DispatchSessions()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                await Task.WhenAll(_dispatchCollection);
                await Task.Delay(TimeSpan.FromMilliseconds(PollTimeInMs), _cancellationTokenSource.Token);
            }
            
        }

        private CancellationTokenSource _cancellationTokenSource;
        private IEnumerable<Task> _dispatchCollection;


        private async void CheckClosedSession()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                foreach (var session in Sessions)
                {
                    var wsSession = (WebSession)session;
                    if (wsSession.State == SessionState.Closed || IsTimeOuted(wsSession))
                    {
                        SessionClose(wsSession);
                    }
                }
                await Task.Delay(TimeSpan.FromMilliseconds(PollTimeInMs), _cancellationTokenSource.Token);
            }
        }

        private bool IsTimeOuted(WebSession session)
        {
            return DateTime.UtcNow - session.LastReceiveMessage > TimeOutConnection;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            try
            {
                await _checkTask;
                await _dispatchTask;
            }
            catch (OperationCanceledException e)
            {
            }
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (context.WebSockets.IsWebSocketRequest && context.Request.Path.Value.Contains(ConnectionPath))
            {
                var request = new WebSocketAspNetRequest(context, await context.WebSockets.AcceptWebSocketAsync(SubProtocol));
                try
                {
                    BeforeSessionOpen(request);
                    var session = await request.WaitOpen;
                    await Task.Yield();
                    SessionOpen(session);
                    await session.WaitClose;
                }
                catch (OperationCanceledException e)
                {
                }
                
            }
            else
            {
                await next(context);
            }
        }
    }
}