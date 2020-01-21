using System;
using LiteNetLib;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetLibOperation;
using NetworkOperation;
using NetworkOperation.Factories;
using NetworkOperation.Infrastructure.Host;
using NetworkOperation.Server;

namespace LiteNet.Infrastructure.Host
{
    public static class HostBuilderExtensions
    {
        public static HostBuilder<TRequest, TResponse> UseLiteNet<TRequest,TResponse>(this HostBuilder<TRequest, TResponse> builder, Action<NetLibHost<TRequest, TResponse>> setup = null) where TRequest : IOperationMessage, new() where TResponse : IOperationMessage, new()
        {
            builder.Service.AddTransient<IFactory<NetManager, MutableSessionCollection>, SessionsFactory>();
            builder.Service.AddSingleton<IHostContext, NetLibHost<TRequest, TResponse>>(p =>
            {
                var host = ActivatorUtilities.CreateInstance<NetLibHost<TRequest, TResponse>>(p);
                setup?.Invoke(host);
                return host;
            });
            builder.Service.AddSingleton(p => (IHostedService)p.GetRequiredService<IHostContext>());
            return builder;
        }
    }
}