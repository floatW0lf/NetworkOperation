using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Web;
using NetworkOperation.Core;
using NetworkOperation.Host;
using NetworkOperation.WebSockets.Core;

namespace NetworkOperation.WebSockets.Host
{
    internal class WebSocketsRequest : SessionRequest
    {
        private readonly HttpListenerWebSocketContext _wsContext;
        private ConcurrentQueue<Session> _queue;
        private EndPoint _remote;
        public override ArraySegment<byte> RequestPayload { get; }

        public WebSocketsRequest(HttpListenerWebSocketContext wsContext, ConcurrentQueue<Session> queue, EndPoint remote)
        {
            _remote = remote;
            _queue = queue;
            _wsContext = wsContext;
            var collection = HttpUtility.ParseQueryString(wsContext.RequestUri.Query);
            if (!collection.HasKeys()) return;
            var payload = collection["payload"];
            if (string.IsNullOrWhiteSpace(payload)) return;
            RequestPayload = Convert.FromBase64String(payload).To();
        }

        
        protected override Session Accepted(IEnumerable<SessionProperty> properties)
        {
            return new WebSession(_wsContext.WebSocket,_remote, properties);
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