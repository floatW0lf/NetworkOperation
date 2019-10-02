using System;
using NetworkOperation;
using NetworkOperation.Host;

namespace NetLibOperation
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