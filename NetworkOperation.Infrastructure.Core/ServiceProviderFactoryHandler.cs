using System;
using Microsoft.Extensions.DependencyInjection;

namespace NetworkOperation.Infrastructure
{
    public class ServiceProviderFactoryHandler : IHandlerFactory
    {
        private readonly IServiceProvider _provider;

        public ServiceProviderFactoryHandler(IServiceProvider provider)
        {
            _provider = provider;
        }
        public IHandler<TOperation, TResult, TRequest> Create<TOperation, TResult, TRequest>() where TOperation : IOperation<TOperation, TResult> where TRequest : IOperationMessage
        {
           return _provider.GetRequiredService<IHandler<TOperation, TResult, TRequest>>();
        }

        public void Destroy(IHandler handler)
        {
        }
    }
}