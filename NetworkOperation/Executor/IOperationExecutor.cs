using System.Threading;
using System.Threading.Tasks;
using NetworkOperation.Core.Models;

namespace NetworkOperation.Core
{
    public interface IOperationExecutor
    {
        Task<OperationResult<TResult>> Execute<TOperation, TResult>(TOperation operation, CancellationToken cancellation = default) where TOperation : IOperation<TResult>;
    }
}