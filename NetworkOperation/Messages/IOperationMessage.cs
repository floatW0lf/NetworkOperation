namespace NetworkOperation
{
    public interface IStatus
    {
        uint StatusCode { get; set; }
    }

    public interface IOperationMessage : IStatus
    {
        int Id { get; set; }
        uint OperationCode { get; set; }
        byte[] OperationData { get; set; }
    }
}