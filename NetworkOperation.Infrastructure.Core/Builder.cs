using System;
using Microsoft.Extensions.DependencyInjection;
using NetworkOperation.Logger;

namespace NetworkOperation.Infrastructure
{
    public abstract class Builder<TRequest,TResponse,TImplement> where TRequest : IOperationMessage, new() where TResponse : IOperationMessage, new() where TImplement : Builder<TRequest,TResponse,TImplement>
    {
        public Builder(IServiceCollection service)
        {
            Service = service;
        }
        public IServiceCollection Service { get; }
        
        public TImplement ConfigureDispatcher<TDispatcher>(Action<TDispatcher> dispatcher = null) where TDispatcher : BaseDispatcher<TRequest,TResponse>
        {
            Service.AddSingleton<BaseDispatcher<TRequest,TResponse>>(p =>
            {
                var d = ActivatorUtilities.CreateInstance<TDispatcher>(p);
                dispatcher?.Invoke(d);
                return d;
            });
            return This;
        }
        
        public TImplement ConfigureSerializer<TSerializer>() where  TSerializer : BaseSerializer
        {
            Service.AddSingleton<BaseSerializer, TSerializer>();
            return This;
        }

        public TImplement ConfigureRuntimeModel(OperationRuntimeModel model)
        {
            Service.Add(ServiceDescriptor.Singleton(typeof(OperationRuntimeModel), model));
            return This;
        }

        public TImplement ConsoleLogger()
        {
            Service.AddSingleton<IStructuralLogger,ConsoleStructuralLogger>();
            return (TImplement)this;
        }
        public TImplement RegisterStatusCodes(params Type[] codes)
        {
            StatusCode.Register(codes);
            return This;
        }

        private TImplement This => (TImplement) this;
    }
}