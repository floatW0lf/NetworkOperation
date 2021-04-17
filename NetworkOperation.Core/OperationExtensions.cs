﻿using System;
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
            return new OperationResult<TResult>(value,BuiltInOperationState.Success); 
        }
        
        public static OperationResult<TResult> ReturnCode<TOperation, TResult, TEnum, TRequest>(this IHandler<TOperation, TResult,TRequest> handler, TEnum code, TResult value = default) where TOperation : IOperation<TResult> where TEnum : Enum where TRequest : IOperationMessage
        {
            return new OperationResult<TResult>(value, StatusCode.FromEnum(code)); 
        }
        
        public static OperationResult<Empty> ReturnEmpty<TOperation, TResult, TRequest>(this IHandler<TOperation, TResult, TRequest> handler) where TOperation : IOperation<TResult> where TRequest : IOperationMessage
        {
            return new OperationResult<Empty>(default,BuiltInOperationState.Success);
        }

        public static ArraySegment<T> To<T>(this T[] array)
        {
            return new ArraySegment<T>(array);
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

        
    }
}