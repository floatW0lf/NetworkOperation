using System;

namespace NetworkOperation.Core
{
    public interface IOperationWithStatus<TResult, TStatus> : IOperation<TResult> where TStatus : Enum { }
    public interface IOperationWithStatus<TResult, TStatus1, TStatus2> : IOperation<TResult> where TStatus1 : Enum where TStatus2 : Enum { }
}