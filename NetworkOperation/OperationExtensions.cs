using System;
using System.Globalization;
using System.Linq;
using NetworkOperation.StatusCodes;

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
            return new OperationResult<TResult>(value,(uint)BuiltInOperationState.Success); 
        }
        
        public static OperationResult<TResult> ReturnCode<TOperation, TResult, TEnum, TRequest>(this IHandler<TOperation, TResult,TRequest> handler, TEnum code, TResult value = default) where TOperation : IOperation<TOperation, TResult> where TEnum : IConvertible where TRequest : IOperationMessage
        {
            if (!StatusEncoding.IsEnumRegistered<TEnum>()) throw new ArgumentException($"{typeof(TEnum)} must be registered. Use {nameof(StatusEncoding)}.{nameof(StatusEncoding.Register)} for registration.");
            return new OperationResult<TResult>(value, code.ToUInt32(CultureInfo.InvariantCulture)); 
        }

        public static ArraySegment<T> To<T>(this T[] array)
        {
            return new ArraySegment<T>(array);
        }
            

        
    }
}