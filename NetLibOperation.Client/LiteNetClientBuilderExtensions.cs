using NetworkOperation;

namespace NetLibOperation.Client
{
    public static class LiteNetClientBuilderExtensions
    {
        public static NetLibClientBuilder<TRequest, TResponse> UseConnectKey<TRequest,TResponse>(this NetLibClientBuilder<TRequest, TResponse> builder, string connectKey) where TRequest : IOperationMessage, new() where TResponse : IOperationMessage, new()
        {
            builder.ConnectionKey = connectKey;
            return builder;
        }
    }
}