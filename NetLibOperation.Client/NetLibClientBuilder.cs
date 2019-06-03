using System;
using System.Collections.Generic;
using System.Linq;
using LiteNetLib;
using NetworkOperation;
using NetworkOperation.Client;
using NetworkOperation.Dispatching;
using NetworkOperation.Factories;
using NetworkOperation.Logger;

namespace NetLibOperation.Client
{
    public class NetLibClientBuilder<TRequest,TResponse> : IClientBuilder<TRequest,TResponse> where TRequest : IOperationMessage, new() where TResponse : IOperationMessage, new()
    {
        private readonly Dictionary<Type,Type> _handlers = new Dictionary<Type, Type>();
        internal Func<ClientBuilderContext,IFactory<NetPeer,Session>> SessionSetup = ctx => new SessionFactory(ctx.HandlerFactory);
        internal Func<ClientBuilderContext,IFactory<Session,IClientOperationExecutor>> ExecutorSetup = context => new DefaultClientOperationExecutorFactory<TRequest,TResponse>(context.Model,context.Serializer, context.StructuralLogger);
       
        internal Func<ClientBuilderContext,BaseDispatcher<TRequest,TResponse>> DispatcherSetup = context => new ExpressionDispatcher<TRequest,TResponse>(context.Serializer,context.HandlerFactory, context.Model,context.StructuralLogger);

        

        public ClientBuilderContext BuilderContext { get; set; } = new ClientBuilderContext();

        public void RegisterHandler<THandler>() where THandler : IHandler
        {
            RegisterHandler(typeof(THandler));
        }

        public void RegisterHandler(Type handler)
        {
            var concreteHandler = handler.GetInterfaces().First(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IHandler<,,>));
            _handlers.Add(concreteHandler, handler);
        }

        public IClient Build()
        {
            if (BuilderContext.Serializer == null) ThrowCannotBuild(nameof(BuilderContext.Serializer));
            
            ((IInterfaceMapAccessor) BuilderContext.HandlerFactory).InterfaceToClassMap = _handlers;
            
            return new Client<TRequest, TResponse>(SessionSetup(BuilderContext), ExecutorSetup(BuilderContext), DispatcherSetup(BuilderContext), BuilderContext.StructuralLogger);
        }

        private static void ThrowCannotBuild(string missing)
        {
            throw new InvalidOperationException($"Cannot build without {missing}");
        }
    }
}