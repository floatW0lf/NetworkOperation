using System.Runtime.Serialization;
using NetworkOperation;

namespace Contract
{
    [DataContract]
    [Operation(0,Handle = Side.Client)]
    public struct ClientOp : IOperation<ClientOp,Empty>
    {
        [DataMember(Order = 0)]
        public string Message;
    }
}