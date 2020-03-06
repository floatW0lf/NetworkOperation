using System.Threading;
using System.Threading.Tasks;

namespace NetworkOperation
{
    public interface IOperationExecutor
    {
        Task<OperationResult<TResult>> Execute<TOperation, TResult>(TOperation operation, CancellationToken cancellation = default) where TOperation : IOperation<TResult>;
    }
}