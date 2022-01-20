using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetworkOperation.Core.Messages;
using NetworkOperation.Host;

namespace NetworkOperation.Infrastructure.Host
{
    public static class HostBuilderExtensions
    {
        public static HostBuilder<TRequest,TResponse> NetworkOperationHost<TRequest,TResponse>(this IServiceCollection collection) where TRequest : IOperationMessage, new() where TResponse : IOperationMessage, new()
        {
            return new HostBuilder<TRequest, TResponse>(collection);
        }
        
        public static HostBuilder<TRequest, TResponse> UseHost<THost,TRequest, TResponse, TConnection>(this HostBuilder<TRequest, TResponse> builder, Func<THost,AbstractHost<TRequest,TResponse,TConnection>> resolver,  Action<THost> setup = null) 
           
            where THost : AbstractHost<TRequest,TResponse,TConnection>
            where TRequest : IOperationMessage, new() 
            where TResponse : IOperationMessage, new() 
        {
            builder.Service.AddSingleton<IHostContext, THost>(p =>
            {
                var host = ActivatorUtilities.CreateInstance<THost>(p);
                setup?.Invoke(host);
                return host;
            });
            builder.Service.AddSingleton(p => (IHostedService)p.GetRequiredService<IHostContext>());
            return builder;
        }
    }
}