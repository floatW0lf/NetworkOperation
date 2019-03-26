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
        public IHandlerFactory HandlerFactory { get; set; } = new DefaultHandlerFactory();
        public BaseSerializer Serializer { get; set; }
        public OperationRuntimeModel Model { get; set; } = OperationRuntimeModel.CreateFromAttribute();
        
        public IStructuralLogger StructuralLogger { get; set; } = new ConsoleStructuralLogger();
        public string ConnectionKey { get; set; }
        
        
        public Func<IClientBuilder<TRequest,TResponse>,IFactory<NetPeer,Session>> SessionSetup { get; set; } = builder => new SessionFactory(builder.HandlerFactory);
        public Func<IClientBuilder<TRequest,TResponse>,IFactory<Session,IClientOperationExecutor>> ExecutorSetup { get; set; } = builder => new DefaultClientOperationExecutorFactory<TRequest,TResponse>(builder.Model,builder.Serializer);
       
        public Func<IClientBuilder<TRequest,TResponse>,BaseDispatcher<TRequest,TResponse>> DispatcherSetup { get; set; } = builder => new ExpressionDispatcher<TRequest,TResponse>(builder.Serializer,builder.HandlerFactory, builder.Model,builder.StructuralLogger);


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
            if (Serializer == null) ThrowCannotBuild(nameof(Serializer));
            if (string.IsNullOrEmpty(ConnectionKey)) ThrowCannotBuild(nameof(ConnectionKey));
            
            ((IInterfaceMapAccessor) HandlerFactory).InterfaceToClassMap = _handlers;
            
            return new Client<TRequest, TResponse>(SessionSetup(this), ExecutorSetup(this), DispatcherSetup(this), StructuralLogger, ConnectionKey);
        }

        private static void ThrowCannotBuild(string missing)
        {
            throw new InvalidOperationException($"Cannot build without {missing}");
        }
    }
}