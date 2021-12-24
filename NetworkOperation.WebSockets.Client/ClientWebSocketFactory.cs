using System;
using System.Net.WebSockets;
using NetworkOperation.Core;
using NetworkOperation.Core.Factories;
using NetworkOperation.WebSockets.Core;

namespace NetworkOperation.WebSockets.Client
{
    public class ClientWebSocketFactory : IFactory<WebSocket,Session>
    {
        public Session Create(WebSocket arg)
        {
            return new WebSession(arg, new byte[4096], Array.Empty<SessionProperty>());
        }
    }
}