namespace NetworkOperation
{
    public interface IResponseReceiver<in TMessage> where TMessage : IOperationMessage
    {
        bool Receive(TMessage defaultOperationMessage);
    }
}