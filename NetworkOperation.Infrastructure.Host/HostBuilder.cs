using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetworkOperation.Core;
using NetworkOperation.Core.Factories;
using NetworkOperation.Core.Messages;
using NetworkOperation.Core.Models;
using NetworkOperation.Host;

namespace NetworkOperation.Infrastructure.Host
{
    public class HostBuilder<TRequest,TResponse> : Builder<TRequest,TResponse,HostBuilder<TRequest,TResponse>> where TRequest : IOperationMessage, new() where TResponse : IOperationMessage, new()
    {
        public HostBuilder(IServiceCollection service) : base(service)
        {
        }

        public HostBuilder<TRequest, TResponse> Executor(Action<HostOperationExecutor<TRequest,TResponse>> setup = null)
        {
            Service.AddTransient<IFactory<SessionCollection, IHostOperationExecutor>,Factory>(p =>
            {
                var f = ActivatorUtilities.GetServiceOrCreateInstance<Factory>(p);
                f.Setup = setup;
                return f;
            });
            return this;
        }

        public HostBuilder<TRequest, TResponse> ConnectHandler<TConnectionRequest>() where TConnectionRequest : SessionRequestHandler
        {
            Service.AddSingleton<SessionRequestHandler, TConnectionRequest>();
            return this;
        }
        
        private class Factory : IFactory<SessionCollection,IHostOperationExecutor>
        {
            private readonly OperationRuntimeModel _model;
            private readonly BaseSerializer _serializer;
            private readonly ILoggerFactory _loggerFactory;

            public Action<HostOperationExecutor<TRequest,TResponse>> Setup { get; set; }
            public Factory(OperationRuntimeModel model, BaseSerializer serializer, ILoggerFactory loggerFactory)
            {
                _model = model;
                _serializer = serializer;
                _loggerFactory = loggerFactory;
            }
            public IHostOperationExecutor Create(SessionCollection arg)
            {
                var e = new HostOperationExecutor<TRequest,TResponse>(_model,_serializer,arg,_loggerFactory);
                Setup?.Invoke(e);
                return e;
            }
        }
    }
}