using Microsoft.Extensions.DependencyInjection;

namespace NetworkOperation.Infrastructure
{
    public interface IBuilder
    {
        IServiceCollection Service { get; }
    }
}