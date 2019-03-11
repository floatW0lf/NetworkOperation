using LiteNetLib;
using NetworkOperation;
using NetworkOperation.Factories;

namespace NetLibOperation
{
    public class SessionFactory : IFactory<NetPeer, Session>
    {
        private readonly IHandlerFactory _handlerFactory;

        public SessionFactory(IHandlerFactory handlerFactory)
        {
            _handlerFactory = handlerFactory;
        }
        public Session Create(NetPeer arg)
        {
            return new NetLibSession(arg, _handlerFactory);
        }
    }
}