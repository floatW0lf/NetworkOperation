namespace NetworkOperation
{
    public struct RequestContext<TRequest> where TRequest : IOperationMessage
    {
        public readonly TRequest Message;
        public readonly Session Session;

        internal RequestContext(TRequest message, Session session)
        {
            Message = message;
            Session = session;
        }
    }
}