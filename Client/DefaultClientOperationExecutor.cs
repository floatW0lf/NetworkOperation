using System.Threading;
using System.Threading.Tasks;

namespace NetworkOperation.Client
{
    public class DefaultClientOperationExecutor<TRequest,TResponse> : BaseOperationExecutor<TRequest,TResponse>, IClientOperationExecutor where TRequest : IOperationMessage, new() where TResponse : IOperationMessage, new()
    {
        private DefaultClientOperationExecutor(OperationRuntimeModel model, BaseSerializer serializer, SessionCollection sessions) : base(model, serializer, sessions)
        {
        }

        public DefaultClientOperationExecutor(OperationRuntimeModel model, BaseSerializer serializer, Session session) : base(model, serializer, session)
        {
        }

        public Task<OperationResult<TResult>> Execute<TOp, TResult>(TOp operation, CancellationToken cancellation = default) where TOp : IOperation<TOp, TResult>
        {
            return SendOperation<TOp, TResult>(operation,null,false, cancellation);
        }
    }
}