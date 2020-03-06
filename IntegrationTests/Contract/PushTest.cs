using System.Runtime.Serialization;
using NetworkOperation;

namespace IntegrationTests.Contract
{
    [DataContract]
    [Operation(1, Handle = Side.Server)]
    public struct PushTest : IOperation<string>
    {
        [DataMember]
        public string Message;
    }
}