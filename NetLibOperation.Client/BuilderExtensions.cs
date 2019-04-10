using System;
using LiteNetLib;
using NetworkOperation;
using NetworkOperation.Client;
using NetworkOperation.Factories;

namespace NetLibOperation.Client
{
    public static class BuilderExtensions
    {
        public static IClientBuilder<TRequest,TResponse> UseSessionFactory<TRequest,TResponse>(this IClientBuilder<TRequest,TResponse> builder,Func<ClientBuilderContext,IFactory<NetPeer,Session>> sessionFactorySetup) where TRequest : IOperationMessage, new() where TResponse : IOperationMessage, new()
        {
            ((NetLibClientBuilder<TRequest, TResponse>) builder).SessionSetup = sessionFactorySetup;
            return builder;
        }
        
        public static IClientBuilder<TRequest,TResponse> UseDispatcherFactory<TRequest,TResponse>(this IClientBuilder<TRequest,TResponse> builder,Func<ClientBuilderContext,BaseDispatcher<TRequest,TResponse>> dispatcherFactorySetup) where TRequest : IOperationMessage, new() where TResponse : IOperationMessage, new()
        {
            ((NetLibClientBuilder<TRequest, TResponse>) builder).DispatcherSetup = dispatcherFactorySetup;
            return builder;
        }
    }
}