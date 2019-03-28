using NetworkOperation.Factories;
using NetworkOperation.Host;
using NetworkOperation.Logger;

namespace NetworkOperation.Server
{
    public class DefaultServerOperationExecutorFactory<TRequest, TResponse> : IFactory<SessionCollection,IHostOperationExecutor> where TRequest : IOperationMessage, new() where TResponse : IOperationMessage, new()
    {
        private readonly OperationRuntimeModel _model;
        private readonly BaseSerializer _serializer;
        private readonly IStructuralLogger _logger;

        public DefaultServerOperationExecutorFactory(OperationRuntimeModel model, BaseSerializer serializer, IStructuralLogger logger)
        {
            _model = model;
            _serializer = serializer;
            _logger = logger;
        }
        public IHostOperationExecutor Create(SessionCollection arg)
        {
            return new HostOperationExecutor<TRequest,TResponse>(_model, _serializer, arg, _logger);
        }
    }
}