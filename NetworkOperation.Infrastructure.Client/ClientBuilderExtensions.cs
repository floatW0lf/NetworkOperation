using Microsoft.Extensions.DependencyInjection;

namespace NetworkOperation.Infrastructure.Client
{
    public static class ClientBuilderExtensions
    {
        public static ClientBuilder<TRequest,TResponse> NetworkOperationClient<TRequest,TResponse>(this IServiceCollection collection) where TRequest : IOperationMessage, new() where TResponse : IOperationMessage, new()
        {
            return new ClientBuilder<TRequest, TResponse>(collection);
        }
    }
}