﻿using System.Threading;
using System.Threading.Tasks;
using NetworkOperation.Core.Messages;
using NetworkOperation.Core.Models;

namespace NetworkOperation.Core
{
    public interface IHandler { }
    public interface IHandler<in TOperation, TResult, TRequest> : IHandler where TOperation : IOperation<TResult> where TRequest : IOperationMessage 
    {
        Task<OperationResult<TResult>> Handle(TOperation objectData, RequestContext<TRequest> context, CancellationToken token);
    }
}