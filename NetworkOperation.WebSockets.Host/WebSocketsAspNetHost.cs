using System;
using System.Net;
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
    public class WebSocketsAspNetHost<TRequest,TResponse> : AbstractHost<TRequest,TResponse, HttpContext>, IMiddleware where TRequest : IOperationMessage, new() where TResponse : IOperationMessage, new()
    {
        public WebSocketsAspNetHost(IFactory<HttpContext, MutableSessionCollection> sessionsFactory, IFactory<SessionCollection, IHostOperationExecutor> executorFactory, BaseDispatcher<TRequest, TResponse> dispatcher, SessionRequestHandler handler, ILoggerFactory loggerFactory) : base(sessionsFactory, executorFactory, dispatcher, handler, loggerFactory)
        {
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                var newWebSocket = await context.WebSockets.AcceptWebSocketAsync();
                var request = new WebSocketAspNetRequest(context, newWebSocket);
                BeforeSessionOpen(request);
                await Task.Yield();
                SessionOpen(request.WaitOpen);
            }
            else
            {
                await next(context);
            }
        }
    }
}