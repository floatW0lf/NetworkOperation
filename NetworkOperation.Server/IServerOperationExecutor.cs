using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkOperation.Server
{
    public interface IServerOperationExecutor : IOperationExecutor
    {
        Task<OperationResult<TOpResult>> Execute<TOp,TOpResult>(TOp operation, IReadOnlyList<Session> receivers, CancellationToken cancellation = default) where TOp : IOperation<TOp,TOpResult>;
    }
}