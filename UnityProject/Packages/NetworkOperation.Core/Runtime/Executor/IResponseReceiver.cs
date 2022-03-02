using NetworkOperation.Core.Messages;

namespace NetworkOperation.Core
{
    public interface IResponseReceiver<in TMessage> where TMessage : IOperationMessage
    {
        bool Receive(TMessage defaultOperationMessage);
    }
}