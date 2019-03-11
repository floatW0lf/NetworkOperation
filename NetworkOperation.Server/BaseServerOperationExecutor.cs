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

        public ServerOperationExecutor(OperationRuntimeModel model, BaseSerializer serializer, SessionCollection sessions, IRequestPlaceHolder<TMessage> messagePlaceHolder) : base(model, serializer, sessions, messagePlaceHolder)
        {
        }

        private ServerOperationExecutor(OperationRuntimeModel model, BaseSerializer serializer, Session session, IRequestPlaceHolder<TMessage> messagePlaceHolder) : base(model, serializer, session, messagePlaceHolder)
        {
        }
    }
}
