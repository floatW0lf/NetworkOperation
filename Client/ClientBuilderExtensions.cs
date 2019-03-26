using System;

namespace NetworkOperation.Client
{
    public static class ClientBuilderExtensions
    {
        public static IClientBuilder<TRequest,TResponse> Register<TRequest,TResponse>(this IClientBuilder<TRequest,TResponse> builder, params Type[] handlers) where TRequest : IOperationMessage, new() where TResponse : IOperationMessage, new()
        {
            foreach (var type in handlers)
            {
                builder.RegisterHandler(type);
            }
            
            return builder;
        }

        public static IClientBuilder<TRequest, TResponse> UseSerializer<TRequest,TResponse>(this IClientBuilder<TRequest,TResponse> builder, BaseSerializer serializer) where TRequest : IOperationMessage, new() where TResponse : IOperationMessage, new()
        {
            builder.Serializer = serializer;
            return builder;
        }
        
        public static IClientBuilder<TRequest, TResponse> UseModel<TRequest,TResponse>(this IClientBuilder<TRequest,TResponse> builder, OperationRuntimeModel model) where TRequest : IOperationMessage, new() where TResponse : IOperationMessage, new()
        {
            builder.Model = model;
            return builder;
        }
        
    }
}