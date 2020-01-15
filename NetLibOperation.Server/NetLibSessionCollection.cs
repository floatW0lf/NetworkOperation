using System;
using System.ComponentModel.Design;
using System.Threading.Tasks;
using LiteNetLib;
using NetworkOperation;

namespace NetLibOperation
{
    internal class NetLibSessionCollection : MutableSessionCollection
    {
        private readonly NetManager _manager;

        public NetLibSessionCollection(NetManager manager)
        {
            _manager = manager;
            Statistics = new LiteNetStatistics(manager.Statistics);
        }

        public override NetworkStatistics Statistics { get; }

        protected override Task SendToAllAsync(ArraySegment<byte> data, DeliveryMode mode)
        {
            _manager.SendToAll(data.Array,data.Offset,data.Count,mode.Convert());
            return Task.CompletedTask;
        }
        
    }
}