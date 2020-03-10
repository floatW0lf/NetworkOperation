using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkOperation.Host
{
    public static class HostOperationExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<OperationResult<TResult>> Execute<TOperation, TResult>(this IHostOperationExecutor executor, TOperation operation, IEnumerable<Session> receivers, Func<TOperation,IOperation<TResult>> resolver, CancellationToken cancellation = default) where TOperation : IOperation<TResult>
        {
            return executor.Execute<TOperation, TResult>(operation, receivers, cancellation);
        }
    }
}