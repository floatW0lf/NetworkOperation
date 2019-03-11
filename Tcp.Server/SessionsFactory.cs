using System.Net.Sockets;
using NetworkOperation;
using NetworkOperation.Factories;

namespace Tcp.Server
{
    public class SessionsFactory : IFactory<Socket,MutableSessionCollection>
    {
        public MutableSessionCollection Create( Socket socket)
        {
            return new TcpSessionCollection();
        }
    }
}