using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using NetworkOperation.Core.Messages;
using NetworkOperation.WebSockets.Host;

namespace NetworkOperation.Infrastructure.Host.AspNet.WebSockets
{
    public static class SetupExtensions
    {
        public static IApplicationBuilder UseNetworkOperationWebSockets<TRequest,TResponse>(this IApplicationBuilder builder) where TRequest : IOperationMessage, new() where TResponse : IOperationMessage, new()
        {
            return builder.UseMiddleware<WebSocketsAspNetHost<TRequest, TResponse>>();
        }
        

        public static IWebHostBuilder NetworkOperationWebSockets<TRequest,TResponse>(this IWebHostBuilder builder, Action<HostBuilder<TRequest,TResponse>> configure, Action<WebSocketsAspNetHost<TRequest,TResponse>> hostConfigure = null) where TRequest : IOperationMessage, new() where TResponse : IOperationMessage, new()
        {
            return builder.ConfigureServices(collection =>
            {
                configure(collection.NetworkOperationHost<TRequest, TResponse>().Executor().UseHost(x => x, hostConfigure));

            });

        }
    }
}