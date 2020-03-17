namespace NetworkOperation.Core.Models
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