namespace NetworkOperation
{
    public interface IOperationMessage
    {
        int Id { get; set; }
        uint OperationCode { get; set; }
        byte[] OperationData { get; set; }
        uint StatusCode { get; set; }
    }
}