using NetworkOperation.Factories;

namespace NetworkOperation.Server
{
    public class DefaultServerOperationExecutorFactory<TRequest, TResponse> : IFactory<SessionCollection,IServerOperationExecutor> where TRequest : IOperationMessage, new() where TResponse : IOperationMessage, new()
    {
        private readonly OperationRuntimeModel _model;
        private readonly BaseSerializer _serializer;
        private readonly IRequestPlaceHolder<TRequest> _messagePlaceHolder;

        public DefaultServerOperationExecutorFactory(OperationRuntimeModel model, BaseSerializer serializer, IRequestPlaceHolder<TRequest> messagePlaceHolder)
        {
            _model = model;
            _serializer = serializer;
            _messagePlaceHolder = messagePlaceHolder;
        }
        public IServerOperationExecutor Create(SessionCollection arg)
        {
            return new ServerOperationExecutor<TRequest,TResponse>(_model, _serializer, arg);
        }
    }
}