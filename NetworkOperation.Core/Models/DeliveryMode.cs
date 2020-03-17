using System;

namespace NetworkOperation.Core.Models
{
    [Flags]
    public enum DeliveryMode
    {
        Unreliable = 0,
        Reliable = 1,
        Ordered = 2,
        Sequenced = 4
    }

    public static class MinRequiredDeliveryMode
    {
        public const DeliveryMode ReliableWithOrdered = DeliveryMode.Reliable | DeliveryMode.Ordered;
    }
}