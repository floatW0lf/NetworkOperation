using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using NetworkOperation.Core.Messages;
using NetworkOperation.Core.Models;

namespace NetworkOperation.Core
{
    public static class OperationExtensions
    {
        public static Type GetResultFromOperation(this Type type)
        {
            var arguments = GetGenericArgsFromInterface(type, typeof(IOperation<>));
            return arguments[0];
        }

        public static Type[] GetGenericArgsFromInterface(this Type type, Type definition)
        {
            var opInterfaceInfo = type.GetInterfaces().First(t =>
                t.IsGenericType && t.GetGenericTypeDefinition() == definition);
            return opInterfaceInfo.GetGenericArguments();
        }

        public static OperationResult<TResult> Return<TOperation, TResult, TRequest>(this IHandler<TOperation, TResult, TRequest> handler, TResult value) where TOperation : IOperation<TResult> where TRequest : IOperationMessage
        {
            return new OperationResult<TResult>(value,new StatusCode(1, (ushort)BuiltInOperationState.Success)); 
        }
        
        public static OperationResult<TResult> ReturnCode<TOperation, TResult, TStatus, TRequest>(this IHandler<TOperation, TResult,TRequest> handler, TStatus code, TResult value = default)
            where TOperation : IOperationWithStatus<TResult,TStatus> 
            where TStatus : Enum 
            where TRequest : IOperationMessage
        {
            return new OperationResult<TResult>(value, new StatusCode(2, Unsafe.As<TStatus,ushort>(ref code))); 
        }
        
        public static OperationResult<TResult> ReturnCode<TOperation, TResult,TRequest, TStatus1, TStatus2>(this IHandler<TOperation, TResult,TRequest> handler, TStatus1 code, Func<TOperation,IOperationWithStatus<TResult,TStatus1,TStatus2>> r, TResult value = default)
            where TOperation : IOperationWithStatus<TResult,TStatus1,TStatus2> 
            where TRequest : IOperationMessage
            where TStatus1 : Enum
            where TStatus2 : Enum
        {
            return new OperationResult<TResult>(value, new StatusCode(2, Unsafe.As<TStatus1,ushort>(ref code))); 
        }
        
        public static OperationResult<TResult> ReturnCode<TOperation, TResult,TRequest, TStatus1, TStatus2>(this IHandler<TOperation, TResult,TRequest> handler, TStatus2 code, Func<TOperation,IOperationWithStatus<TResult,TStatus1,TStatus2>> r, TResult value = default)
            where TOperation : IOperationWithStatus<TResult,TStatus1,TStatus2> 
            where TRequest : IOperationMessage
            where TStatus1 : Enum
            where TStatus2 : Enum
        {
            return new OperationResult<TResult>(value, new StatusCode(3, Unsafe.As<TStatus2,ushort>(ref code))); 
        }
        
        public static OperationResult<Empty> ReturnEmpty<TOperation, TResult, TRequest>(this IHandler<TOperation, TResult, TRequest> handler) where TOperation : IOperation<TResult> where TRequest : IOperationMessage
        {
            return new OperationResult<Empty>(default,new StatusCode(1, (ushort)BuiltInOperationState.Success)); 
        }

        public static ArraySegment<T> To<T>(this T[] array)
        {
            return new ArraySegment<T>(array);
        }
        public static ArraySegment<T> Slice<T>(in this ArraySegment<T> segment, int index, int count)
        {
            if ((uint)index > (uint)segment.Count || (uint)count > (uint)(segment.Count - index))
            {
                throw new ArgumentException();
            }
            return new ArraySegment<T>(segment.Array!, segment.Offset + index, count);
        }
        
        
        /// <summary>
        /// Method for help compiler resolve TResult
        /// o => o
        /// </summary>
        /// <param name="executor"></param>
        /// <param name="operation"></param>
        /// <param name="resolver">use like as o => o</param>
        /// <param name="cancellation"></param>
        /// <typeparam name="TOperation"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<OperationResult<TResult>> Execute<TOperation, TResult>(this IOperationExecutor executor, TOperation operation, Func<TOperation,IOperation<TResult>> resolver, CancellationToken cancellation = default) where TOperation : IOperation<TResult>
        {
            return executor.Execute<TOperation, TResult>(operation, cancellation);
        }
        
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<OperationResultExtended<TOperation,TResult>> Execute<TOperation, TResult,TStatus>(this IOperationExecutor executor, TOperation operation, Func<TOperation,IOperationWithStatus<TResult,TStatus>> resolver, CancellationToken cancellation = default) where TOperation : IOperation<TResult> where TStatus : Enum
        {
            var result = await executor.Execute<TOperation, TResult>(operation, cancellation);
            return new OperationResultExtended<TOperation, TResult>(result);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<OperationResultExtended<TOperation,TResult>> Execute<TOperation, TResult,TStatus1, TStatus2>(this IOperationExecutor executor, TOperation operation, Func<TOperation,IOperationWithStatus<TResult,TStatus1,TStatus2>> resolver, CancellationToken cancellation = default) where TOperation : IOperation<TResult> where TStatus1 : Enum where TStatus2 : Enum
        {
            var result = await executor.Execute<TOperation, TResult>(operation, cancellation);
            return new OperationResultExtended<TOperation, TResult>(result);
        }

        
    }
}