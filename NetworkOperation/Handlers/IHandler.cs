using System.Threading;
using System.Threading.Tasks;

namespace NetworkOperation
{
    public interface IHandler { }
    public interface IHandler<in TOperation, TResult, TRequest> : IHandler where TOperation : IOperation<TOperation,TResult> where TRequest : IOperationMessage 
    {
        Task<OperationResult<TResult>> Handle(TOperation objectData, RequestContext<TRequest> context, CancellationToken token);
    }
}