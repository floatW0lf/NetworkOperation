
namespace NetworkOperation
{
    public struct OperationResult<T>
    {
        internal OperationResult(T result, StatusCode status)
        {
            Result = result;
            Status = status;
        }
        
        public T Result { get; }
        public StatusCode Status { get; }

        public override string ToString()
        {
            return $"{nameof(Result)}: {Result}, {nameof(Status)}: {Status}";
        }
    }
}