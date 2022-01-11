using System;
using NetworkOperation.Core;
using NetworkOperation.Core.Factories;

namespace WebGL.WebSockets
{
    public class SessionFactory : IFactory<WebSocket, Session>
    {
        public Session Create(WebSocket arg)
        {
            return new WebSocketSession(arg, Array.Empty<SessionProperty>());
        }
    }
}