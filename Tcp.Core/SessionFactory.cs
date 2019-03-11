using System.Net.Sockets;
using NetworkOperation;
using NetworkOperation.Factories;

namespace Tcp.Core
{
    public class SessionFactory : IFactory<Socket,Session>
    {
        private readonly IHandlerFactory _handlerFactory;

        public SessionFactory(IHandlerFactory handlerFactory)
        {
            _handlerFactory = handlerFactory;
        }
            
        public Session Create(Socket arg)
        {
            return new TcpSession(arg, _handlerFactory); 
        }
    }
}