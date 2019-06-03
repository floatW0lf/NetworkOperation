using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NetworkOperation.Host;
using NetworkOperation.Logger;

namespace NetworkOperation.Server
{
    public sealed class HostOperationExecutor<TMessage, TResponse> : BaseOperationExecutor<TMessage,TResponse>, IHostOperationExecutor where TMessage : IOperationMessage, new() where TResponse : IOperationMessage, new()
    {

        public Task<OperationResult<TOpResult>> Execute<TOp, TOpResult>(TOp operation, CancellationToken cancellation = default) where TOp : IOperation<TOp, TOpResult>
        {
            return SendOperation<TOp, TOpResult>(operation, null, true, cancellation);
        }

        public Task<OperationResult<TOpResult>> Execute<TOp, TOpResult>(TOp operation, IReadOnlyList<Session> receivers, CancellationToken cancellation = default) where TOp : IOperation<TOp, TOpResult>
        {
            return SendOperation<TOp, TOpResult>(operation, receivers, false, cancellation);
        }


        public HostOperationExecutor(OperationRuntimeModel model, BaseSerializer serializer, SessionCollection sessions, IStructuralLogger logger) : base(model, serializer, sessions, logger)
        {
        }

        private HostOperationExecutor(OperationRuntimeModel model, BaseSerializer serializer, Session session, IStructuralLogger logger) : base(model, serializer, session, logger)
        {
        }
    }
}
