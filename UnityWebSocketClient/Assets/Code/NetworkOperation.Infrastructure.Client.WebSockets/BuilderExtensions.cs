using System;
using Microsoft.Extensions.DependencyInjection;
using NetworkOperation.Client;
using NetworkOperation.Core;
using NetworkOperation.Core.Factories;
using NetworkOperation.Core.Messages;
using NetworkOperation.WebSockets.Client;
using WebGL.WebSockets;
using WNetworkOperation.WebSockets.Client;

namespace NetworkOperation.Infrastructure.Client.WebSockets
{
    public static class BuilderExtensions
    {
        public static ClientBuilder<TRequest, TResponse> UseWebSockets<TRequest, TResponse>(this ClientBuilder<TRequest, TResponse> builder, Action<WebSocketClient<TRequest, TResponse>> action = null) where TRequest : IOperationMessage, new() where TResponse : IOperationMessage, new()
        {
            builder.Service.AddSingleton<IFactory<WebSocket, Session>, SessionFactory>();
            builder.Service.AddSingleton<IClient>(p =>
            {
                var c = ActivatorUtilities.CreateInstance<WebSocketClient<TRequest, TResponse>>(p);
                action?.Invoke(c);
                return c;
            });
            builder.Service.AddSingleton(p => (ISessionEvents) p.GetService<IClient>());
            return builder;
        }   
    }
}