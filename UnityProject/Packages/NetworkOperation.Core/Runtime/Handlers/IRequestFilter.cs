using System.Threading.Tasks;
using NetworkOperation.Core.Messages;
using NetworkOperation.Core.Models;

namespace NetworkOperation.Core
{
    public interface IRequestFilter<TRequest,TResponse> where TRequest : IOperationMessage
    {
        Task<TResponse> Handle(RequestContext<TRequest> context);
    }
}