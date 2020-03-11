using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace NetworkOperation.Infrastructure
{
    public abstract class Builder<TRequest,TResponse,TImplement> : IBuilder where TRequest : IOperationMessage, new() where TResponse : IOperationMessage, new() where TImplement : Builder<TRequest,TResponse,TImplement>
    {
        private DescriptionRuntimeModel _runtimeModel;
        public Builder(IServiceCollection service)
        {
            Service = service;
            Service.AddSingleton<IHandlerFactory, ServiceProviderFactoryHandler>();
            _runtimeModel = new DescriptionRuntimeModel();
            Service.Add(new ServiceDescriptor(typeof(DescriptionRuntimeModel),_runtimeModel));
        }
        public IServiceCollection Service { get; }
        
        public TImplement Dispatcher<TDispatcher>(Action<TDispatcher> dispatcher = null) where TDispatcher : BaseDispatcher<TRequest,TResponse>
        {
            Service.AddSingleton<BaseDispatcher<TRequest,TResponse>>(p =>
            {
                var d = ActivatorUtilities.CreateInstance<TDispatcher>(p);
                dispatcher?.Invoke(d);
                return d;
            });
            return This;
        }
        
        public TImplement Serializer<TSerializer>() where  TSerializer : BaseSerializer
        {
            Service.AddSingleton<BaseSerializer, TSerializer>();
            return This;
        }

        public TImplement RuntimeModel(OperationRuntimeModel model)
        {
            Service.Add(ServiceDescriptor.Singleton(typeof(OperationRuntimeModel), model));
            return This;
        }

        public TImplement RegisterHandlers(IEnumerable<Assembly> assemblies, Scope lifetime = Scope.Single)
        {
            return RegisterHandlers(assemblies.SelectMany(a => a.GetTypes()), lifetime);
        }
        public TImplement RegisterHandlers(IEnumerable<Type> anyTypes,Scope lifetime = Scope.Single)
        {
            var handlers = anyTypes.Where(t => !t.IsAbstract && !t.IsInterface && typeof(IHandler).IsAssignableFrom(t));

            foreach (var handler in handlers)
            {
                RegisterHandler(handler, handler.GetCustomAttribute<HandlerAttribute>(true)?.LifeTime ?? lifetime);
            }
            return This;
        }
        public TImplement RegisterHandler<THandler>(Scope lifetime) where THandler : IHandler
        {
            return RegisterHandler(typeof(THandler), lifetime);
        }
        public TImplement RegisterHandler(Type handler, Scope lifetime)
        {
            Type interfaceHandler;
            try
            {
                interfaceHandler = handler.GetInterfaces().First(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IHandler<,,>));
            }
            catch (InvalidOperationException)
            {
                throw new InvalidOperationException($"{handler} not implement interface {typeof(IHandler<,,>)}");
            }
            if (interfaceHandler.GetGenericArguments()[2] != typeof(TRequest))
            {
                throw new InvalidOperationException($"Invalid request type for handler {handler}. Must be {typeof(TRequest)}");
            } 
            _runtimeModel.Register(interfaceHandler.GetGenericArguments()[0], new HandlerDescription(lifetime));
            Service.Add(ServiceDescriptor.Describe(interfaceHandler,handler, Convert(lifetime)));
            return This;
        }

        private ServiceLifetime Convert(Scope scope)
        {
            switch (scope)
            {
                case Scope.Single:
                    return ServiceLifetime.Singleton;
                case Scope.Session:
                    return ServiceLifetime.Scoped;
                case Scope.Request:
                    return ServiceLifetime.Transient;
                default:
                    throw new ArgumentOutOfRangeException(nameof(scope), scope, null);
            }
        }
        public TImplement RegisterStatusCodes(params Type[] codes)
        {
            StatusCode.Register(codes);
            return This;
        }

        private TImplement This => (TImplement) this;
    }
}