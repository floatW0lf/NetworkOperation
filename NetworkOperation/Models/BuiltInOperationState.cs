namespace NetworkOperation
{
    public enum BuiltInOperationState : uint
    {
        None = 0,
        InternalError,
        Handle,
        Success,
        NoWaiting,
        Cancel = 50
    }
}