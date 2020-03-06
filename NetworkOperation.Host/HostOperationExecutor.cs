using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetworkOperation.Dispatching;

namespace NetworkOperation.Host
{
    public sealed class HostOperationExecutor<TMessage, TResponse> : BaseOperationExecutor<TMessage,TResponse>, IHostOperationExecutor where TMessage : IOperationMessage, new() where TResponse : IOperationMessage, new()
    {
        private readonly SessionCollection _sessions;

        public Task<OperationResult<TOpResult>> Execute<TOp, TOpResult>(TOp operation, CancellationToken cancellation = default) where TOp : IOperation<TOpResult>
        {
            return SendOperation<TOp, TOpResult>(operation, null, cancellation);
        }

        public Task<OperationResult<TOpResult>> Execute<TOp, TOpResult>(TOp operation, IEnumerable<Session> receivers, CancellationToken cancellation = default) where TOp : IOperation<TOpResult>
        {
            return SendOperation<TOp, TOpResult>(operation, receivers,  cancellation);
        }

        public HostOperationExecutor(OperationRuntimeModel model, BaseSerializer serializer, SessionCollection sessions, ILoggerFactory loggerFactory) : base(model, serializer, loggerFactory)
        {
            _sessions = sessions;
        }

        protected override async Task SendRequest(IEnumerable<Session> receivers, byte[] request, DeliveryMode mode)
        {
            var requestWithMessageType = request.AppendInBegin(TypeMessage.Request);
            if (receivers == null)
            {
                await _sessions.SendToAllAsync(requestWithMessageType, mode);
                return;
            }
            if (ParallelServerRequestSend)
            {
                await Task.WhenAll(receivers.Select(s => s.SendMessageAsync(requestWithMessageType, mode)));
            }
            else
            {
                if (receivers is IReadOnlyList<Session> list)
                {
                    // ReSharper disable once ForCanBeConvertedToForeach
                    for (int i = 0; i < list.Count; i++)
                    {
                        await list[i].SendMessageAsync(requestWithMessageType, mode);
                    }
                    return;
                }

                foreach (var receiver in receivers)
                {
                    await receiver.SendMessageAsync(requestWithMessageType, mode);
                }
                
            }
        }

        public bool ParallelServerRequestSend { get; set; }
    }
}
