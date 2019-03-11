using System.Runtime.Serialization;
using NetworkOperation;

namespace Contract
{
    [DataContract]
    [Operation(2,Handle = Side.All)]
    public struct Minus : IOperation<Minus,float>
    {
        [DataMember]
        public float A;
        [DataMember]
        public float B;
    }

    [DataContract]
    [Operation(4, Handle = Side.All)]
    public class Multiplay : IOperation<Multiplay, float>
    {
        [DataMember]
        public float A;
        [DataMember]
        public float B;
    }
}