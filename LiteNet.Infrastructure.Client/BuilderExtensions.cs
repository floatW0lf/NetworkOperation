using System;
using LiteNetLib;
using Microsoft.Extensions.DependencyInjection;
using NetLibOperation;
using NetLibOperation.Client;
using NetworkOperation;
using NetworkOperation.Client;
using NetworkOperation.Factories;
using NetworkOperation.Infrastructure.Client;

namespace LiteNet.Infrastructure.Client
{
    public static class BuilderExtensions
    {
        public static ClientBuilder<TRequest, TResponse> UseLiteNet<TRequest, TResponse>(this ClientBuilder<TRequest, TResponse> builder, Action<Client<TRequest, TResponse>> action = null) where TRequest : IOperationMessage, new() where TResponse : IOperationMessage, new()
        {
            builder.Service.AddSingleton<IFactory<NetPeer, Session>, SessionFactory>();
            builder.Service.AddSingleton<IClient>(p =>
            {
                var c = ActivatorUtilities.CreateInstance<Client<TRequest, TResponse>>(p);
                action?.Invoke(c);
                return c;
            });
            builder.Service.AddSingleton(p => (ISessionEvents) p.GetService<IClient>());
            return builder;
        }   
    }
}