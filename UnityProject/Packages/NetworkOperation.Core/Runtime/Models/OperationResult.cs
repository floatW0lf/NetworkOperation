
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace NetworkOperation.Core.Models
{
    public struct OperationResult<T>
    {
        internal OperationResult(T result, StatusCode status)
        {
            Result = result;
            Status = status;
        }
        
        public T Result { get; }
        public StatusCode Status { get; }

        public override string ToString()
        {
            return $"{nameof(Result)}: {Result}, {nameof(Status)}: {Status}";
        }
    }
    public readonly struct OperationResultExtended<TOperation,TResult> where TOperation : IOperation<TResult>
    {
        internal readonly OperationResult<TResult> InnerResult;
        public OperationResultExtended(OperationResult<TResult> result)
        {
            InnerResult = result;
        }
        public TResult Result => InnerResult.Result;


        public override string ToString()
        {
            return $"Status : {InnerResult.Status.TypeTag}";
        }
    }

    public static class OperationResultExtensions
    {
        public static bool Is<TResult>(in this OperationResult<TResult> self, BuiltInOperationState status)
        {
            return self.Status.TypeTag == 1 && self.Status.EnumValue == (ushort)status;
        }
        public static bool Is<TOperation,TResult,TStatus>(in this OperationResultExtended<TOperation,TResult> self, TStatus status) where TOperation : IOperationWithStatus<TResult,TStatus> where TStatus : Enum
        {
            return self.InnerResult.Status.TypeTag == 2 && self.InnerResult.Status.EnumValue == Unsafe.As<TStatus,ushort>(ref status);
        }
        public static bool Is<TOperation,TResult,TStatus1,TStatus2>(in this OperationResultExtended<TOperation,TResult> self, TStatus1 status, Func<TOperation,IOperationWithStatus<TResult,TStatus1,TStatus2>> r) 
            where TOperation : IOperationWithStatus<TResult,TStatus1,TStatus2> 
            where TStatus1 : Enum 
            where TStatus2 : Enum
        {
            return self.InnerResult.Status.TypeTag == 2 && self.InnerResult.Status.EnumValue == Unsafe.As<TStatus1,ushort>(ref status);
        }

        public static bool Is<TOperation,TResult,TStatus1,TStatus2>(in this OperationResultExtended<TOperation,TResult> self, TStatus2 status, Func<TOperation,IOperationWithStatus<TResult,TStatus1,TStatus2>> r) 
            where TOperation : IOperationWithStatus<TResult,TStatus1,TStatus2> 
            where TStatus1 : Enum 
            where TStatus2 : Enum
        {
            return self.InnerResult.Status.TypeTag == 3 && self.InnerResult.Status.EnumValue == Unsafe.As<TStatus2,ushort>(ref status);
        }
        public static void Match<TOperation,TResult,TStatus>(in this OperationResultExtended<TOperation,TResult> self, Action<BuiltInOperationState,TResult> first, Action<TStatus,TResult> second, Action @default = null) where TOperation : IOperationWithStatus<TResult,TStatus> where TStatus : Enum
        {
            var value = self.InnerResult.Status.EnumValue;
            switch (self.InnerResult.Status.TypeTag)
            {
                case 1: first((BuiltInOperationState) value, self.Result);
                    break;
                case 2: second(Unsafe.As<ushort, TStatus>(ref value), self.Result);
                    break;
                default:
                    @default?.Invoke();
                    break;
            }
        }
        
        public static void Match<TOperation,TResult,TStatus1, TStatus2>(in this OperationResultExtended<TOperation,TResult> self,Func<TOperation,IOperationWithStatus<TResult,TStatus1,TStatus2>> r,Action<BuiltInOperationState,TResult> first, Action<TStatus1,TResult> second, Action<TStatus2,TResult> third, Action @default = null) 
            where TOperation : IOperationWithStatus<TResult,TStatus1,TStatus2> 
            where TStatus1 : Enum
            where TStatus2 : Enum
        {
            var value = self.InnerResult.Status.EnumValue;
            switch (self.InnerResult.Status.TypeTag)
            {
                case 1: first((BuiltInOperationState) value, self.Result);
                    break;
                case 2: second(Unsafe.As<ushort, TStatus1>(ref value), self.Result);
                    break;
                case 3: third(Unsafe.As<ushort, TStatus2>(ref value), self.Result);
                    break;
                default:
                    @default?.Invoke();
                    break;
            }
        }
        public static TZipResult Zip<TOperation,TResult,TStatus1, TStatus2,TZipResult>(in this OperationResultExtended<TOperation,TResult> self,Func<TOperation,IOperationWithStatus<TResult,TStatus1,TStatus2>> r,Func<BuiltInOperationState,TResult,TZipResult> first, Func<TStatus1,TResult,TZipResult> second, Func<TZipResult> @default = null) 
            where TOperation : IOperationWithStatus<TResult,TStatus1,TStatus2> 
            where TStatus1 : Enum
            where TStatus2 : Enum
        {
            var value = self.InnerResult.Status.EnumValue;
            switch (self.InnerResult.Status.TypeTag)
            {
                case 1: return first((BuiltInOperationState) value, self.Result);
                case 2: return second(Unsafe.As<ushort, TStatus1>(ref value), self.Result);
                default:
                    return @default == null ? default : @default();
            }
        }
        
        public static TZipResult Zip<TOperation,TResult,TStatus1, TStatus2,TZipResult>(in this OperationResultExtended<TOperation,TResult> self,Func<TOperation,IOperationWithStatus<TResult,TStatus1,TStatus2>> r,Func<BuiltInOperationState,TResult,TZipResult> first, Func<TStatus1,TResult,TZipResult> second, Func<TStatus2,TResult,TZipResult> third, Func<TZipResult> @default = null) 
            where TOperation : IOperationWithStatus<TResult,TStatus1,TStatus2> 
            where TStatus1 : Enum
            where TStatus2 : Enum
        {
            var value = self.InnerResult.Status.EnumValue;
            switch (self.InnerResult.Status.TypeTag)
            {
                case 1: return first((BuiltInOperationState) value, self.Result);
                    
                case 2: return second(Unsafe.As<ushort, TStatus1>(ref value), self.Result);
                    
                case 3: return third(Unsafe.As<ushort, TStatus2>(ref value), self.Result);
                    
                default:
                    return @default == null ? default : @default();
            }
        }
    }
}