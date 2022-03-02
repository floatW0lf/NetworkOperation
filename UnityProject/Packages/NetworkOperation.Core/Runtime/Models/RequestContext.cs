using NetworkOperation.Core.Messages;

namespace NetworkOperation.Core.Models
{
    public struct RequestContext<TRequest> where TRequest : IOperationMessage
    {
        public readonly TRequest Message;
        public readonly Session Session;
        public readonly OperationDescription OperationDescription;
        public readonly HandlerDescription HandlerDescription;

        internal RequestContext(TRequest message, Session session, OperationDescription operationDescription, HandlerDescription handlerDescription)
        {
            Message = message;
            Session = session;
            OperationDescription = operationDescription;
            HandlerDescription = handlerDescription;
        }
    }
}