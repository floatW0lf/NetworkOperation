using System;
using NetworkOperation.Logger;

namespace NetworkOperation.Client
{
    public interface IClientBuilder<TRequest,TResponse> 
        where TRequest : IOperationMessage, new() 
        where TResponse : IOperationMessage, new()
    {
        IHandlerFactory HandlerFactory { get; set; }
        BaseSerializer Serializer { get; set; }
        OperationRuntimeModel Model { get; set; }
        IStructuralLogger StructuralLogger { get; set; }
        void RegisterHandler<THandler>() where THandler : IHandler;
        void RegisterHandler(Type handler);
        IClient Build();
    }
}