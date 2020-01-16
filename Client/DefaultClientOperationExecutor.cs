using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NetworkOperation.Dispatching;
using NetworkOperation.Logger;

namespace NetworkOperation.Client
{
    public class DefaultClientOperationExecutor<TRequest,TResponse> : BaseOperationExecutor<TRequest,TResponse>, IClientOperationExecutor where TRequest : IOperationMessage, new() where TResponse : IOperationMessage, new()
    {
        private readonly Session _session;
        
        public Task<OperationResult<TResult>> Execute<TOp, TResult>(TOp operation, CancellationToken cancellation = default) where TOp : IOperation<TOp, TResult>
        {
            return SendOperation<TOp, TResult>(operation,null, cancellation);
        }

        public DefaultClientOperationExecutor(OperationRuntimeModel model, BaseSerializer serializer, Session session, IStructuralLogger logger) : base(model, serializer, logger)
        {
            _session = session;
        }

        protected override async Task SendRequest(IEnumerable<Session> receivers, byte[] request, DeliveryMode mode)
        {
            await _session.SendMessageAsync(request.AppendInBegin(TypeMessage.Request), mode);
        }
    }
}