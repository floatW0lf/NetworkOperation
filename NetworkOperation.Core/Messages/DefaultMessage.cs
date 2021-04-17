﻿using System.Runtime.Serialization;

namespace NetworkOperation.Core.Messages
{
    [DataContract]
    public struct DefaultMessage : IOperationMessage
    {
        [DataMember(Order = 0)]
        public int Id { get; set; }
        [DataMember(Order = 1)]
        public uint OperationCode { get; set; }
        [DataMember(Order = 2)]
        public byte[] OperationData { get; set; }
        [DataMember(Order = 3)]
        public StatusCode Status { get; set; }
    }
}