using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetworkOperation.Core.Messages;
using NetworkOperation.Host;
using NetworkOperation.WebSockets.Host;

namespace NetworkOperation.Infrastructure.Host.WebSockets
{
    public static class HostBuilderExtensions
    {
        public static HostBuilder<TRequest, TResponse> UseWebSockets<TRequest, TResponse>(this HostBuilder<TRequest, TResponse> builder, Action<WebSocketsHost<TRequest, TResponse>> setup = null) where TRequest : IOperationMessage, new() where TResponse : IOperationMessage, new()
        {
            builder.Service.AddSingleton<IHostContext, WebSocketsHost<TRequest, TResponse>>(p =>
            {
                var host = ActivatorUtilities.CreateInstance<WebSocketsHost<TRequest, TResponse>>(p);
                setup?.Invoke(host);
                return host;
            });
            builder.Service.AddSingleton(p => (IHostedService)p.GetRequiredService<IHostContext>());
            return builder;
        }
    }
}