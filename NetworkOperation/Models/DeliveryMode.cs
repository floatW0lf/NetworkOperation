using System;

namespace NetworkOperation
{
    [Flags]
    public enum DeliveryMode
    {
        Unreliable = 0,
        Reliable = 1,
        Ordered = 1 << 1,
        Sequenced = 1 << 2
    }

    public static class MinRequiredDeliveryMode
    {
        public const DeliveryMode ReliableWithOrdered = DeliveryMode.Reliable | DeliveryMode.Ordered;
    }
}