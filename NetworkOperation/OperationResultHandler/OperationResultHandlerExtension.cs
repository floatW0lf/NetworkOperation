namespace NetworkOperation.OperationResultHandler
{
    public static class OperationResultHandlerExtension
    {
        public static OperationResultHandler<T> Handle<T>(this OperationResult<T> operationResult)
        {
            return new OperationResultHandler<T>(operationResult, false);
        }
    }
}