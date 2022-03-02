using System;
using LiteNetLib;
using Microsoft.Extensions.DependencyInjection;
using NetLibOperation.LiteNet;
using NetworkOperation.Client;
using NetworkOperation.Core;
using NetworkOperation.Core.Factories;
using NetworkOperation.Core.Messages;
using NetworkOperation.LiteNet.Client;

namespace NetworkOperation.Infrastructure.Client.LiteNet
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