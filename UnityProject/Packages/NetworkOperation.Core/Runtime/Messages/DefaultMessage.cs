using System.Runtime.Serialization;
using NetworkOperation.Core.Dispatching;

namespace NetworkOperation.Core.Messages
{
    [DataContract]
    public struct DefaultMessage : IOperationMessage
    {
        [DataMember(Order = 0)]
        public TypeMessage Type { get; set; }
        [DataMember(Order = 1)]
        public int Id { get; set; }
        [DataMember(Order = 2)]
        public uint OperationCode { get; set; }
        [DataMember(Order = 3)]
        public byte[] OperationData { get; set; }
        [DataMember(Order = 4)]
        public StatusCode Status { get; set; }
        
    }
}