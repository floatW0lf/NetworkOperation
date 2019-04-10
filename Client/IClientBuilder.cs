using System;
using NetworkOperation.Logger;

namespace NetworkOperation.Client
{
    public interface IClientBuilder<TRequest,TResponse> 
        where TRequest : IOperationMessage, new() 
        where TResponse : IOperationMessage, new()
    {
        ClientBuilderContext BuilderContext { get; set; }
        
        void RegisterHandler<THandler>() where THandler : IHandler;
        void RegisterHandler(Type handler);
        IClient Build();
    }
}