using LiteNetLib;
using NetworkOperation;
using NetworkOperation.Factories;

namespace NetLibOperation
{
    public class SessionsFactory : IFactory<NetManager,MutableSessionCollection>
    {
        public MutableSessionCollection Create(NetManager connections)
        {
            return new NetLibSessionCollection(connections);
        }
    }
}