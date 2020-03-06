﻿namespace NetworkOperation
{
    public interface IHandlerFactory
    {
        IHandler<TOperation,TResult,TRequest> Create<TOperation,TResult,TRequest>(RequestContext<TRequest> requestContext) where TOperation : IOperation<TResult> where TRequest : IOperationMessage;
        void Destroy(IHandler handler);
    }
}