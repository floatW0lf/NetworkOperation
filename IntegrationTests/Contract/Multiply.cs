using System.Runtime.Serialization;
using NetworkOperation;

namespace IntegrationTests.Contract
{
    [DataContract]
    [Operation(4, Handle = Side.All)]
    public class Multiply : IOperation<Multiply, float>
    {
        [DataMember(Order = 0)]
        public float A;
        [DataMember(Order = 1)]
        public float B;
    }
}