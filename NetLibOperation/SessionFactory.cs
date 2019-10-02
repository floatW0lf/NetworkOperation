using System;
using LiteNetLib;
using NetworkOperation;
using NetworkOperation.Factories;

namespace NetLibOperation
{
    public class SessionFactory : IFactory<NetPeer, Session>
    {
        public SessionFactory()
        {
        }
        public Session Create(NetPeer arg)
        {
            return new NetLibSession(arg, Array.Empty<SessionProperty>());
        }
    }
}