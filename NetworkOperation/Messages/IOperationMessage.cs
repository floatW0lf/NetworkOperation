namespace NetworkOperation
{
    public interface IOperationMessage
    {
        uint OperationCode { get; set; }
        byte[] OperationData { get; set; }
        uint StateCode { get; set; }
    }
}