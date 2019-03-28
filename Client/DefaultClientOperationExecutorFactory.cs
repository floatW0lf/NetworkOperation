using NetworkOperation.Factories;
using NetworkOperation.Logger;

namespace NetworkOperation.Client
{
    public class DefaultClientOperationExecutorFactory<TRequest,TResponse> : IFactory<Session, IClientOperationExecutor> where TResponse : IOperationMessage, new() where TRequest : IOperationMessage, new()
    {
        private readonly OperationRuntimeModel _model;
        private readonly BaseSerializer _serializer;
        private readonly IStructuralLogger _structuralLogger;

        public DefaultClientOperationExecutorFactory(OperationRuntimeModel model, BaseSerializer serializer,IStructuralLogger structuralLogger)
        {
            _model = model;
            _serializer = serializer;
            _structuralLogger = structuralLogger;
        }
        public IClientOperationExecutor Create(Session arg)
        {
            return new DefaultClientOperationExecutor<TRequest,TResponse>(_model,_serializer,arg,_structuralLogger);
        }
    }
}