using System;
using System.Net.Sockets;
using NetworkOperation.Core;
using NetworkOperation.Core.Factories;

namespace Tcp.Core
{
    public class SessionFactory : IFactory<Socket,Session>
    {
        public SessionFactory()
        {
        }
            
        public Session Create(Socket arg)
        {
            return new TcpSession(arg, Array.Empty<SessionProperty>()); 
        }
    }
}