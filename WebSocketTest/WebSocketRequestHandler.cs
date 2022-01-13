using System.Collections.Generic;
using NetworkOperation.Core;
using NetworkOperation.Host;

namespace WebSocketTest
{
    public class WebSocketRequestHandler : SessionRequestHandler
    {
        public WebSocketRequestHandler(BaseSerializer serializer) : base(serializer)
        {
        }

        public override void Handle(SessionRequest request)
        {
            request.Accept(new List<SessionProperty>());
        }
    }
}