using Microsoft.Extensions.Logging;
using NetworkOperation.Core;
using NetworkOperation.Core.Factories;
using NetworkOperation.Core.Messages;
using NetworkOperation.Core.Models;

namespace NetworkOperation.Host
{
    public class DefaultServerOperationExecutorFactory<TRequest, TResponse> : IFactory<SessionCollection,IHostOperationExecutor> where TRequest : IOperationMessage, new() where TResponse : IOperationMessage, new()
    {
        private readonly OperationRuntimeModel _model;
        private readonly BaseSerializer _serializer;
        private readonly ILoggerFactory _loggerFactory;

        public DefaultServerOperationExecutorFactory(OperationRuntimeModel model, BaseSerializer serializer, ILoggerFactory loggerFactory)
        {
            _model = model;
            _serializer = serializer;
            _loggerFactory = loggerFactory;
        }
        public IHostOperationExecutor Create(SessionCollection arg)
        {
            return new HostOperationExecutor<TRequest,TResponse>(_model, _serializer, arg, _loggerFactory);
        }
    }
}