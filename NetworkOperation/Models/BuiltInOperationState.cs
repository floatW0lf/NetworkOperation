namespace NetworkOperation
{
    public enum BuiltInOperationState : uint
    {
        None = 0,
        InternalError,
        Success,
        NoWaiting,
        Cancel = 50
    }
}