using System.Threading;
using System.Threading.Tasks;
using NetworkOperation.Logger;

namespace NetworkOperation.Client
{
    public class DefaultClientOperationExecutor<TRequest,TResponse> : BaseOperationExecutor<TRequest,TResponse>, IClientOperationExecutor where TRequest : IOperationMessage, new() where TResponse : IOperationMessage, new()
    {
        

        public Task<OperationResult<TResult>> Execute<TOp, TResult>(TOp operation, CancellationToken cancellation = default) where TOp : IOperation<TOp, TResult>
        {
            return SendOperation<TOp, TResult>(operation,null,false, cancellation);
        }

        private DefaultClientOperationExecutor(OperationRuntimeModel model, BaseSerializer serializer, SessionCollection sessions, IStructuralLogger logger) : base(model, serializer, sessions, logger)
        {
        }

        public DefaultClientOperationExecutor(OperationRuntimeModel model, BaseSerializer serializer, Session session, IStructuralLogger logger) : base(model, serializer, session, logger)
        {
        }
    }
}