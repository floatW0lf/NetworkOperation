namespace NetworkOperation
{
    public enum BuiltInOperationState : ushort
    {
        None = 0,
        InternalError,
        Success,
        NoWaiting,
        Cancel
    }
}