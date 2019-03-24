namespace NetworkOperation
{
    public enum BuiltInOperationState : uint
    {
        None = 0,
        InternalError,
        Handle,
        Success,
        Nowaiting,
        Cancel = 50
    }
}