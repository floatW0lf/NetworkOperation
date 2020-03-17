namespace NetworkOperation.Core.Messages
{
    public interface IStatus
    {
        StatusCode Status { get; set; }
    }

    public interface IOperationMessage : IStatus
    {
        int Id { get; set; }
        uint OperationCode { get; set; }
        byte[] OperationData { get; set; }
    }
}