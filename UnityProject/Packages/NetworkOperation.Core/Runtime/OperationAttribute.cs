using System;
using NetworkOperation.Core.Models;

namespace NetworkOperation.Core
{
    public class OperationAttribute : Attribute
    {
        public OperationAttribute(uint code)
        {
            Code = code;
        }
        public uint Code { get; }
        public Side Handle { get; set; } = Side.All;
        public bool UseAsyncSerialize { get; set; }

        public DeliveryMode ForRequest { get; set; } = DeliveryMode.Reliable | DeliveryMode.Ordered;
        public DeliveryMode ForResponse { get; set; } = DeliveryMode.Reliable | DeliveryMode.Ordered;
        public bool WaitResponse { get; set; } = true;

    }
}