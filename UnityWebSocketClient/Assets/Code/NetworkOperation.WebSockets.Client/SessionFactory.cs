using System;
using NetworkOperation.Core;
using NetworkOperation.Core.Factories;
using NetworkOperation.WebSockets.Client;
using WebGL.WebSockets;

namespace WNetworkOperation.WebSockets.Client
{
    public class SessionFactory : IFactory<WebSocket, Session>
    {
        public Session Create(WebSocket arg)
        {
            return new WebSocketSession(arg, Array.Empty<SessionProperty>());
        }
    }
}