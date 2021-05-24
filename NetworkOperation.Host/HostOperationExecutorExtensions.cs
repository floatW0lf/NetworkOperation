using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using NetworkOperation.Core;
using NetworkOperation.Core.Models;

namespace NetworkOperation.Host
{
    public static class HostOperationExecutorExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<OperationResult<TResult>> Execute<TOperation, TResult>(this IHostOperationExecutor executor, TOperation operation, IEnumerable<Session> receivers, Func<TOperation,IOperation<TResult>> resolver, CancellationToken cancellation = default) where TOperation : IOperation<TResult>
        {
            return executor.Execute<TOperation, TResult>(operation, receivers, cancellation);
        }
        
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<OperationResultExtended<TOperation,TResult>> Execute<TOperation, TResult,TStatus>(this IHostOperationExecutor executor, TOperation operation, IEnumerable<Session> receivers, Func<TOperation,IOperationWithStatus<TResult,TStatus>> resolver, CancellationToken cancellation = default) where TOperation : IOperation<TResult> where TStatus : Enum
        {
            var result = await executor.Execute<TOperation, TResult>(operation, receivers, cancellation);
            return new OperationResultExtended<TOperation, TResult>(result);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<OperationResultExtended<TOperation,TResult>> Execute<TOperation, TResult,TStatus1, TStatus2>(this IHostOperationExecutor executor, TOperation operation, IEnumerable<Session> receivers, Func<TOperation,IOperationWithStatus<TResult,TStatus1,TStatus2>> resolver, CancellationToken cancellation = default) where TOperation : IOperation<TResult> where TStatus1 : Enum where TStatus2 : Enum
        {
            var result = await executor.Execute<TOperation, TResult>(operation, receivers, cancellation);
            return new OperationResultExtended<TOperation, TResult>(result);
        }
    }
}