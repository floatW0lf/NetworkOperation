using System;
using Microsoft.Extensions.DependencyInjection;

namespace NetworkOperation.Infrastructure
{
    public class ServiceProviderFactoryHandler : IHandlerFactory
    {
        private const string ScopeProvider = "_scopeProvider";
        public IServiceScopeFactory Factory { get; }
        private readonly IServiceProvider _provider;

        public ServiceProviderFactoryHandler(IServiceProvider provider, IServiceScopeFactory factory)
        {
            Factory = factory;
            _provider = provider;
        }
        
        public IHandler<TOperation, TResult, TRequest> Create<TOperation, TResult, TRequest>(RequestContext<TRequest> requestContext) where TOperation : IOperation<TOperation, TResult> where TRequest : IOperationMessage
        {
            var provider = _provider;
            if (requestContext.HandlerDescription.LifeTime == Scope.Session)
            {
                if (requestContext.Session[ScopeProvider] == null)
                {
                    requestContext.Session[ScopeProvider] = Factory.CreateScope();
                }
                provider = ((IServiceScope)requestContext.Session[ScopeProvider]).ServiceProvider;
            }
            return provider.GetRequiredService<IHandler<TOperation, TResult, TRequest>>();
        }

        public void Destroy(IHandler handler)
        {
            (handler as IDisposable)?.Dispose();
        }
    }
}