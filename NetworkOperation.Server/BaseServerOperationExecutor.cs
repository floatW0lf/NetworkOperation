using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkOperation.Server
{
    public sealed class ServerOperationExecutor<TMessage, TResponse> : BaseOperationExecutor<TMessage,TResponse>, IServerOperationExecutor where TMessage : IOperationMessage, new() where TResponse : IOperationMessage, new()
    {

        public Task<OperationResult<TOpResult>> Execute<TOp, TOpResult>(TOp operation, CancellationToken cancellation = default) where TOp : IOperation<TOp, TOpResult>
        {
            return SendOperation<TOp, TOpResult>(operation, null, true, cancellation);
        }

        public Task<OperationResult<TOpResult>> Execute<TOp, TOpResult>(TOp operation, IReadOnlyList<Session> receivers, CancellationToken cancellation = default) where TOp : IOperation<TOp, TOpResult>
        {
            return SendOperation<TOp, TOpResult>(operation, receivers, false, cancellation);
        }

        public ServerOperationExecutor(OperationRuntimeModel model, BaseSerializer serializer, SessionCollection sessions) : base(model, serializer, sessions)
        {
        }

        private ServerOperationExecutor(OperationRuntimeModel model, BaseSerializer serializer, Session session) : base(model, serializer, session)
        {
        }
    }
}
