namespace NetworkOperation
{
    public struct OperationContext<TRequest> where TRequest : IOperationMessage
    {
        public readonly TRequest Message;
        public readonly Session Session;

        internal OperationContext(TRequest message, Session session)
        {
            Message = message;
            Session = session;
        }
    }
}