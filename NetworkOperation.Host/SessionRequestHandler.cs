using NetworkOperation.Core;

namespace NetworkOperation.Host
{
    public abstract class SessionRequestHandler
    {
        private readonly BaseSerializer _serializer;

        protected SessionRequestHandler(BaseSerializer serializer)
        {
            _serializer = serializer;
        }

        protected T ReadPayload<T>(SessionRequest request) where T : IConnectPayload
        {
           return _serializer.Deserialize<T>(request.RequestPayload);
        }

        protected void RejectWithPayload<T>(SessionRequest request,T payload) where T : IDisconnectPayload
        {
            request.Reject(_serializer.Serialize(payload).To());
        }
        
        public abstract void Handle(SessionRequest request);
    }
}