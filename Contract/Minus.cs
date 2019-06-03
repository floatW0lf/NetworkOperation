using System.Runtime.Serialization;
using NetworkOperation;

namespace Contract
{
    [DataContract]
    [Operation(2,Handle = Side.All)]
    public struct Minus : IOperation<Minus,float>
    {
        [DataMember(Order = 0)]
        public float A;
        [DataMember(Order = 1)]
        public float B;
    }

    [DataContract]
    [Operation(4, Handle = Side.All)]
    public class Multiplay : IOperation<Multiplay, float>
    {
        [DataMember(Order = 0)]
        public float A;
        [DataMember(Order = 1)]
        public float B;
    }
}