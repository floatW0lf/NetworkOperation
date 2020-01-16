using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkOperation.Host
{
    public interface IHostOperationExecutor : IOperationExecutor
    {
        Task<OperationResult<TOpResult>> Execute<TOp,TOpResult>(TOp operation, IEnumerable<Session> receivers, CancellationToken cancellation = default) where TOp : IOperation<TOp,TOpResult>;
    }
}