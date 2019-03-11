using NetworkOperation.StatusCodes;

namespace NetworkOperation
{
    public struct OperationResult<T>
    {
        internal OperationResult(T result, uint statusCode)
        {
            Result = result;
            StatusCode = statusCode;
        }
        
        public T Result { get; }
        public uint StatusCode { get; }

        public override string ToString()
        {
            return $"{nameof(Result)}: {Result}, {nameof(StatusCode)}: {StatusEncoding.AsString(StatusCode)}";
        }
    }
}