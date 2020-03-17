using System;
using LiteNetLib;
using NetworkOperation.Core;
using NetworkOperation.Core.Factories;

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