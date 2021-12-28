using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using NetworkOperation.Core;
using NetworkOperation.Host;
using NetworkOperation.WebSockets.Core;

namespace NetworkOperation.WebSockets.Host
{
    internal class WebSocketsRequest : SessionRequest
    {
        private readonly HttpListenerWebSocketContext _wsContext; public override ArraySegment<byte> RequestPayload { get; }

        public WebSocketsRequest(HttpListenerWebSocketContext wsContext)
        {
            _wsContext = wsContext;
            RequestPayload = Convert.FromBase64String(wsContext.Headers["_payload"]).To();
        }

        protected override Session Accepted(IEnumerable<SessionProperty> properties)
        {
            WaitOpenSession = new WebSession(_wsContext.WebSocket, properties);
            return WaitOpenSession;
        }
        
        public Session WaitOpenSession { get; private set; }

        public override void Reject(ArraySegment<byte> payload = default)
        {
            _wsContext.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, payload != default ? Convert.ToBase64String(payload.Array, payload.Offset, payload.Count) : string.Empty, default).GetAwaiter();
        }
    }
}