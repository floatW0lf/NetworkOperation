using System.Runtime.Serialization;
using NetworkOperation;

namespace IntegrationTests.Contract
{
    [DataContract]
    [Operation(1, Handle = Side.Server)]
    public struct PlusOp : IOperation<PlusOp, float>
    {
        [DataMember(Order = 0)]
        public float A;
        [DataMember(Order = 1)]
        public float B;
    }
}