using System.Runtime.Serialization;
using NetworkOperation.Core;

namespace IntegrationTests.Contract
{
    public enum MultiplyStatus
    {
        OverFlow,
        Error
    }
    public enum MultiplyStatusExt
    {
        BigResult
    }
    [DataContract]
    [Operation(4, Handle = Side.All)]
    public class Multiply : IOperationWithStatus<float,MultiplyStatus,MultiplyStatusExt>
    {
        [DataMember(Order = 0)]
        public float A;
        [DataMember(Order = 1)]
        public float B;
    }
}