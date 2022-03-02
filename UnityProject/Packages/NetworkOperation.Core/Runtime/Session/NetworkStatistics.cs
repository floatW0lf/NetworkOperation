namespace NetworkOperation.Core
{
    public abstract class NetworkStatistics
    {
        public abstract ulong ReceivedBytes { get; }
        public abstract ulong SentBytes { get; }
        public abstract ulong SentPackets { get; }
        public abstract ulong ReceivedPackets { get; }
        public abstract ulong this[string name] { get; }
        
    }
}