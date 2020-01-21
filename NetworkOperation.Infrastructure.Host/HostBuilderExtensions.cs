using Microsoft.Extensions.DependencyInjection;

namespace NetworkOperation.Infrastructure.Host
{
    public static class HostBuilderExtensions
    {
        public static HostBuilder<TRequest,TResponse> NetworkOperationHost<TRequest,TResponse>(this IServiceCollection collection) where TRequest : IOperationMessage, new() where TResponse : IOperationMessage, new()
        {
            return new HostBuilder<TRequest, TResponse>(collection);
        }
    }
}