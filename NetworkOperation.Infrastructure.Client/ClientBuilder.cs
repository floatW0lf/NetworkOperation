using System;
using Microsoft.Extensions.DependencyInjection;
using NetworkOperation.Client;
using NetworkOperation.Factories;
using NetworkOperation.Logger;

namespace NetworkOperation.Infrastructure.Client
{
    public class ClientBuilder<TRequest,TResponse> : Builder<TRequest,TResponse,ClientBuilder<TRequest,TResponse>> where TRequest : IOperationMessage, new() where TResponse : IOperationMessage, new()
    {
        public ClientBuilder(IServiceCollection service) : base(service)
        {
        }
        public ClientBuilder<TRequest,TResponse> Executor(Action<DefaultClientOperationExecutor<TRequest, TResponse>> action = null)
        {
            Service.AddTransient<IFactory<Session, IClientOperationExecutor>,Factory>(p =>
            {
                var f = ActivatorUtilities.GetServiceOrCreateInstance<Factory>(p);
                f.Configure = action;
                return f;
            });
            return this;
        }
        
        private class Factory : IFactory<Session, IClientOperationExecutor>
        {
            private readonly OperationRuntimeModel _model;
            private readonly BaseSerializer _serializer;
            private readonly IStructuralLogger _logger;
            public Action<DefaultClientOperationExecutor<TRequest, TResponse>> Configure { get; set; }
            
            public Factory(OperationRuntimeModel model, BaseSerializer serializer, IStructuralLogger logger)
            {
                _model = model;
                _serializer = serializer;
                _logger = logger;
            }
            public IClientOperationExecutor Create(Session arg)
            {
                var executor = new DefaultClientOperationExecutor<TRequest,TResponse>(_model,_serializer,arg,_logger);
                Configure?.Invoke(executor);
                return executor;
            }
        }

        
    }
}