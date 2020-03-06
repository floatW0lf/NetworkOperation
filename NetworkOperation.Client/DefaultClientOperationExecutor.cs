using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetworkOperation.Dispatching;

namespace NetworkOperation.Client
{
    public class DefaultClientOperationExecutor<TRequest,TResponse> : BaseOperationExecutor<TRequest,TResponse>, IClientOperationExecutor where TRequest : IOperationMessage, new() where TResponse : IOperationMessage, new()
    {
        private readonly Session _session;
        
        public Task<OperationResult<TResult>> Execute<TOp, TResult>(TOp operation, CancellationToken cancellation = default) where TOp : IOperation<TResult>
        {
            return SendOperation<TOp, TResult>(operation,null, cancellation);
        }

        public DefaultClientOperationExecutor(OperationRuntimeModel model, BaseSerializer serializer, Session session, ILoggerFactory loggerFactory) : base(model, serializer, loggerFactory)
        {
            _session = session;
        }

        protected override async Task SendRequest(IEnumerable<Session> receivers, byte[] request, DeliveryMode mode)
        {
            await _session.SendMessageAsync(request.AppendInBegin(TypeMessage.Request), mode);
        }
    }
}