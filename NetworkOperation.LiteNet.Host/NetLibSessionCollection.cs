using System;
using System.Threading.Tasks;
using LiteNetLib;
using NetLibOperation;
using NetworkOperation.Core;
using NetworkOperation.Core.Models;

namespace NetworkOperation.LiteNet.Host
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

        protected override Task SendToAllAsync(ReadOnlyMemory<byte> data, DeliveryMode mode)
        {
            _manager.SendToAll(data.ToArray(),mode.Convert());
            return Task.CompletedTask;
        }
        
    }
}