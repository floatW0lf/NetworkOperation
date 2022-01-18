using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Web;
using Microsoft.AspNetCore.Http;
using NetworkOperation.Core;
using NetworkOperation.Host;
using NetworkOperation.WebSockets.Core;

namespace NetworkOperation.WebSockets.Host
{
    internal class WebSocketAspNetRequest : SessionRequest
    {
        private WebSocket _webSocket;
        private HttpContext _context;

        public WebSocketAspNetRequest(HttpContext context, WebSocket webSocket)
        {
            _context = context;
            _webSocket = webSocket;
            if (context.Request.Query.TryGetValue("payload", out var values) && values.Count == 0 && !string.IsNullOrWhiteSpace(values[0]))
            {
                RequestPayload = Convert.FromBase64String(values[0]).To();
            }
        }
        public override ArraySegment<byte> RequestPayload { get; }
        protected override Session Accepted(IEnumerable<SessionProperty> properties)
        {
           return WaitOpen = new WebSession(_webSocket, new IPEndPoint(_context.Connection.RemotePort, _context.Connection.RemotePort), properties);
        }
        public Session WaitOpen { get; private set; }
        public override void Reject(ArraySegment<byte> payload = default)
        {
            
        }
    }
}