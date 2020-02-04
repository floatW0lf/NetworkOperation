using System;
using System.Linq;

namespace NetworkOperation.Extensions
{
    public static class OperationExtensions
    {
        public static Type[] GetGenericArgsFromOperation(this Type type)
        {
            var arguments = GetGenericArgsFromInterface(type, typeof(IOperation<,>));
            if (type != arguments[0]) throw new InvalidOperationException($"Invalid operation declare in {type}. Must be {type} : {typeof(IOperation<,>).Name}<{type},{arguments[1]}>{{...}}");
            return arguments;
        }

        public static Type[] GetGenericArgsFromInterface(this Type type, Type definition)
        {
            var opInterfaceInfo = type.GetInterfaces().First(t =>
                t.IsGenericType && t.GetGenericTypeDefinition() == definition);
            return opInterfaceInfo.GetGenericArguments();
        }

        public static OperationResult<TResult> Return<TOperation, TResult, TRequest>(this IHandler<TOperation, TResult, TRequest> handler, TResult value) where TOperation : IOperation<TOperation, TResult> where TRequest : IOperationMessage
        {
            return new OperationResult<TResult>(value,BuiltInOperationState.Success); 
        }
        
        public static OperationResult<TResult> ReturnCode<TOperation, TResult, TEnum, TRequest>(this IHandler<TOperation, TResult,TRequest> handler, TEnum code, TResult value = default) where TOperation : IOperation<TOperation, TResult> where TEnum : Enum where TRequest : IOperationMessage
        {
            return new OperationResult<TResult>(value, StatusCode.FromEnum(code)); 
        }
        
        public static OperationResult<Empty> ReturnEmpty<TOperation, TResult, TRequest>(this IHandler<TOperation, TResult, TRequest> handler) where TOperation : IOperation<TOperation, TResult> where TRequest : IOperationMessage
        {
            return new OperationResult<Empty>(default,BuiltInOperationState.Success);
        }

        public static ArraySegment<T> To<T>(this T[] array)
        {
            return new ArraySegment<T>(array);
        }
            

        
    }
}