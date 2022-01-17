using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetworkOperation.Core;
using NetworkOperation.Core.Messages;
using NetworkOperation.Core.Models;

namespace NetworkOperation.Host
{
    public sealed class HostOperationExecutor<TMessage, TResponse> : BaseOperationExecutor<TMessage,TResponse>, IHostOperationExecutor where TMessage : IOperationMessage, new() where TResponse : IOperationMessage, new()
    {
        private readonly SessionCollection _sessions;

        public Task<OperationResult<TOpResult>> Execute<TOp, TOpResult>(TOp operation, CancellationToken cancellation = default) where TOp : IOperation<TOpResult>
        {
            if (_sessions.Count <= 0) return Task.FromResult<OperationResult<TOpResult>>(default);
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
            if (receivers == null)
            {
                await _sessions.SendToAllAsync(request.To(), mode);
                return;
            }
            if (ParallelServerRequestSend)
            {
                await Task.WhenAll(receivers.Select(s => s.SendMessageAsync(request.To(), mode)));
            }
            else
            {
                if (receivers is IReadOnlyList<Session> list)
                {
                    // ReSharper disable once ForCanBeConvertedToForeach
                    for (int i = 0; i < list.Count; i++)
                    {
                        await list[i].SendMessageAsync(request.To(), mode);
                    }
                    return;
                }

                foreach (var receiver in receivers)
                {
                    await receiver.SendMessageAsync(request.To(), mode);
                }
                
            }
        }

        public bool ParallelServerRequestSend { get; set; }
    }
}
