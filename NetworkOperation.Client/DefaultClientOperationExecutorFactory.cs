using NetworkOperation.Factories;

namespace NetworkOperation.Client
{
    public class DefaultClientOperationExecutorFactory<TRequest,TResponse> : IFactory<Session,IClientOperationExecutor> where TRequest : IOperationMessage, new() where TResponse : IOperationMessage, new()
    {
        private readonly BaseSerializer _serializer;
        private readonly OperationRuntimeModel _model;

        public DefaultClientOperationExecutorFactory(BaseSerializer serializer, OperationRuntimeModel model)
        {
            _serializer = serializer;
            _model = model;
        }
        public IClientOperationExecutor Create(Session arg)
        {
            return new ClientOperationExecutor<TRequest,TResponse>(_model,_serializer, arg);
        }
    }
}