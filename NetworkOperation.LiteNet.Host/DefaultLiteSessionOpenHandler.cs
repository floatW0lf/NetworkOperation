using System;
using NetworkOperation.Core;
using NetworkOperation.Host;

namespace NetworkOperation.LiteNet.Host
{
    public class DefaultLiteSessionOpenHandler : SessionRequestHandler
    {
        public DefaultLiteSessionOpenHandler(BaseSerializer serializer) : base(serializer)
        {
            
        }
        
        public sealed override void Handle(SessionRequest request)
        {
            OnHandle(request);
        }

        protected virtual void OnHandle(SessionRequest request)
        {
            request.Accept(Array.Empty<SessionProperty>());
        }

    }
}