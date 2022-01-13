using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using NetworkOperation.Core;
using NetworkOperation.Host;
using NetworkOperation.WebSockets.Core;

namespace NetworkOperation.WebSockets.Host
{
    internal class WebSocketsRequest : SessionRequest
    {
        private readonly HttpListenerWebSocketContext _wsContext;
        private ConcurrentQueue<Session> _queue;
        public override ArraySegment<byte> RequestPayload { get; }

        public WebSocketsRequest(HttpListenerWebSocketContext wsContext, ConcurrentQueue<Session> queue)
        {
            _queue = queue;
            _wsContext = wsContext;
            RequestPayload = Convert.FromBase64String(wsContext.Headers["_payload"]).To();
        }

        protected override Session Accepted(IEnumerable<SessionProperty> properties)
        {
           return new WebSession(_wsContext.WebSocket, properties);
        }
        protected override void AfterAccept(Session session)
        {
            _queue.Enqueue(session);
        }

        public override void Reject(ArraySegment<byte> payload = default)
        {
            _wsContext.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, payload != default ? Convert.ToBase64String(payload.Array, payload.Offset, payload.Count) : string.Empty, default).GetAwaiter();
        }
    }
}