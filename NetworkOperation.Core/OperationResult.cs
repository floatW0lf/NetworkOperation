
namespace NetworkOperation.Core
{
    public interface IOperationError
    { 
        string Message { get; set; }
    }
    
    public struct OperationResult<TResult,TError> where TError : IOperationError
    {
        public bool IsSuccess { get; set; }
        public TResult Result { get; set; }
        public TError Error { get; set; }
    }

    public struct MessageError : IOperationError
    {
        public MessageError(string message)
        {
            Message = message;
        }
        public string Message { get; set; }
    }
    public interface IOperation<TResult, TError> : IOperation where TError : IOperationError
    {
    }
    
    public interface INewOperation<TResult> : IOperation<TResult,MessageError>
    {
        
    }
}