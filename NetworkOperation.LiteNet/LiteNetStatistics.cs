using System;
using LiteNetLib;
using NetworkOperation.Core;

namespace NetworkOperation.LiteNet
{
    internal class LiteNetStatistics : NetworkStatistics
    {
        private readonly NetStatistics _liteStat;

        public LiteNetStatistics(NetStatistics liteStat)
        {
            _liteStat = liteStat;
        }

        public override ulong ReceivedBytes => (ulong)_liteStat.BytesReceived;
        public override ulong SentBytes => (ulong)_liteStat.BytesSent;
        public override ulong SentPackets =>(ulong) _liteStat.PacketsSent;
        public override ulong ReceivedPackets => (ulong)_liteStat.PacketsReceived;

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
                    case nameof(NetStatistics.PacketLoss): return (ulong)_liteStat.PacketLoss;
                    case nameof(NetStatistics.PacketLossPercent): return (ulong)_liteStat.PacketLossPercent;
                    default: throw new NotSupportedException(name);
                }
            }
        }
        
    }
}