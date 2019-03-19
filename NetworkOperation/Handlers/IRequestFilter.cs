using System.Threading.Tasks;

namespace NetworkOperation
{
    public interface IRequestFilter<TRequest,TResponse> where TRequest : IOperationMessage
    {
        Task<TResponse> Handle(RequestContext<TRequest> context);
    }
}