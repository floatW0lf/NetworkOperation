using System.Runtime.Serialization;
using NetworkOperation;

namespace Contract
{
    [DataContract]
    [Operation(1, Handle = Side.Server)]
    public struct PlusOp : IOperation<PlusOp, float>
    {
        [DataMember]
        public float A;
        [DataMember]
        public float B;
    }
}