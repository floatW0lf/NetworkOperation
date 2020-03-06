using System.Runtime.Serialization;
using NetworkOperation;

namespace IntegrationTests.Contract
{
    [DataContract]
    [Operation(0,Handle = Side.Client)]
    public struct ClientOp : IOperation<string>
    {
        [DataMember(Order = 0)]
        public string Message;
    }
}