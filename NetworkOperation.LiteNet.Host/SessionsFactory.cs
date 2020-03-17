using LiteNetLib;
using NetworkOperation.Core;
using NetworkOperation.Core.Factories;

namespace NetworkOperation.LiteNet.Host
{
    public class SessionsFactory : IFactory<NetManager,MutableSessionCollection>
    {
        public MutableSessionCollection Create(NetManager connections)
        {
            return new NetLibSessionCollection(connections);
        }
    }
}