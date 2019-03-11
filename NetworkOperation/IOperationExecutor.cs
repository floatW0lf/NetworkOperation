using System.Threading;
using System.Threading.Tasks;

namespace NetworkOperation
{
    public interface IOperationExecutor
    {
        Task<OperationResult<TResult>> Execute<TOp, TResult>(TOp operation, CancellationToken cancellation = default) where TOp : IOperation<TOp, TResult>;
    }
}