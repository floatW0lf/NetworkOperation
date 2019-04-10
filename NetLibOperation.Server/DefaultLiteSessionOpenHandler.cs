using LiteNetLib;
using NetworkOperation;
using NetworkOperation.Factories;
using NetworkOperation.Host;

namespace NetLibOperation
{
    public class DefaultLiteSessionOpenHandler : SessionRequestHandler
    {
        private readonly IFactory<NetPeer,Session> _sessionFactory;

        public DefaultLiteSessionOpenHandler(IFactory<NetPeer,Session> sessionFactory, BaseSerializer serializer) : base(serializer)
        {
            _sessionFactory = sessionFactory;
        }
        
        public sealed override void Handle(SessionRequest request)
        {
            ((LiteSessionRequest) request).SessionFactory = _sessionFactory;
            OnHandle(request);
        }

        protected virtual void OnHandle(SessionRequest request)
        {
            request.Accept();
        }

    }
}