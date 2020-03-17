using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NetworkOperation.Core;
using NetworkOperation.Core.Models;

namespace NetworkOperation.Host
{
    public interface IHostOperationExecutor : IOperationExecutor
    {
        Task<OperationResult<TResult>> Execute<TOperation,TResult>(TOperation operation, IEnumerable<Session> receivers, CancellationToken cancellation = default) where TOperation : IOperation<TResult>;
    }
}