using System.Threading.Tasks;
using LiteNetLib;
using NetworkOperation;

namespace NetLibOperation
{
    public class NetLibSessionCollection : MutableSessionCollection
    {
        private readonly NetManager _manager;

        public NetLibSessionCollection(NetManager manager)
        {
            _manager = manager;
        }

        protected override Task SendToAllAsync(byte[] data)
        {
            _manager.SendToAll(data,DeliveryMethod.ReliableOrdered);
            return Task.CompletedTask;
        }
    }
}