using NetworkOperation.Factories;
using NetworkOperation.Host;

namespace NetworkOperation.Server
{
    public class DefaultServerOperationExecutorFactory<TRequest, TResponse> : IFactory<SessionCollection,IHostOperationExecutor> where TRequest : IOperationMessage, new() where TResponse : IOperationMessage, new()
    {
        private readonly OperationRuntimeModel _model;
        private readonly BaseSerializer _serializer;

        public DefaultServerOperationExecutorFactory(OperationRuntimeModel model, BaseSerializer serializer)
        {
            _model = model;
            _serializer = serializer;
        }
        public IHostOperationExecutor Create(SessionCollection arg)
        {
            return new HostOperationExecutor<TRequest,TResponse>(_model, _serializer, arg);
        }
    }
}