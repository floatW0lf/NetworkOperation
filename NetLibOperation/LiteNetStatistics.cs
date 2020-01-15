using System;
using LiteNetLib;
using NetworkOperation;

namespace NetLibOperation
{
    public class LiteNetStatistics : NetworkStatistics
    {
        private readonly NetStatistics _liteStat;

        public LiteNetStatistics(NetStatistics liteStat)
        {
            _liteStat = liteStat;
        }

        public override ulong ReceivedBytes => _liteStat.BytesReceived;
        public override ulong SentBytes => _liteStat.BytesSent;
        public override ulong SentPackets => _liteStat.PacketsSent;
        public override ulong ReceivedPackets => _liteStat.PacketsReceived;

        public override string ToString()
        {
            return _liteStat.ToString();
        }


        public override ulong this[string name]
        {
            get
            {
                switch (name)
                {
                    case nameof(NetStatistics.PacketLoss): return _liteStat.PacketLoss;
                    case nameof(NetStatistics.SequencedPacketLoss): return _liteStat.SequencedPacketLoss;
                    case nameof(NetStatistics.PacketLossPercent): return _liteStat.PacketLossPercent;
                    default: throw new NotSupportedException(name);
                }
            }
        }
        
    }
}