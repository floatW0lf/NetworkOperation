using NetworkOperation.Factories;

namespace NetworkOperation.Client
{
    public class DefaultClientOperationExecutorFactory<TRequest,TResponse> : IFactory<Session, IClientOperationExecutor> where TResponse : IOperationMessage, new() where TRequest : IOperationMessage, new()
    {
        private readonly OperationRuntimeModel _model;
        private readonly BaseSerializer _serializer;

        public DefaultClientOperationExecutorFactory(OperationRuntimeModel model, BaseSerializer serializer)
        {
            _model = model;
            _serializer = serializer;
        }
        public IClientOperationExecutor Create(Session arg)
        {
            return new DefaultClientOperationExecutor<TRequest,TResponse>(_model,_serializer,arg);
        }
    }
}