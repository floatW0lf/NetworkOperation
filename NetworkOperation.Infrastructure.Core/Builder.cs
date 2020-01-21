using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace NetworkOperation.Infrastructure
{
    public abstract class Builder<TRequest,TResponse,TImplement> where TRequest : IOperationMessage, new() where TResponse : IOperationMessage, new() where TImplement : Builder<TRequest,TResponse,TImplement>
    {
        public Builder(IServiceCollection service)
        {
            Service = service;
            Service.AddSingleton<IHandlerFactory, ServiceProviderFactoryHandler>();
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

        public TImplement RegisterHandler<THandler>(ServiceLifetime lifetime) where THandler : IHandler
        {
            var handler = typeof(THandler);
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
                throw new InvalidOperationException($"Invalid request type for handler {typeof(THandler)}. Must be {typeof(TRequest)}");
            } 
            
            Service.Add(ServiceDescriptor.Describe(interfaceHandler,handler, lifetime));
            return This;
        }
        public TImplement RegisterStatusCodes(params Type[] codes)
        {
            StatusCode.Register(codes);
            return This;
        }

        private TImplement This => (TImplement) this;
    }
}