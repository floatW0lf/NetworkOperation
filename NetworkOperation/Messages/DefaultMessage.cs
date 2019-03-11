using System.Runtime.Serialization;

namespace NetworkOperation
{
    [DataContract]
    public struct DefaultMessage : IOperationMessage
    {
        [DataMember]
        public uint OperationCode { get; set; }
        [DataMember]
        public byte[] OperationData { get; set; }
        [DataMember]
        public uint StateCode { get; set; }
    }
}