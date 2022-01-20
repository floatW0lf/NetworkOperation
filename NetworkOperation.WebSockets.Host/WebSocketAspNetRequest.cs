using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
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
        private TaskCompletionSource<WebSession> _completionSource;

        public WebSocketAspNetRequest(HttpContext context, WebSocket webSocket)
        {
            _context = context;
            _webSocket = webSocket;
            if (context.Request.Query.TryGetValue("payload", out var values) && values.Count == 0 && !string.IsNullOrWhiteSpace(values[0]))
            {
                RequestPayload = Convert.FromBase64String(values[0]).To();
            }
            _completionSource = new TaskCompletionSource<WebSession>();
        }
        public override ArraySegment<byte> RequestPayload { get; }
        protected override Session Accepted(IEnumerable<SessionProperty> properties)
        {
           return new WebSession(_webSocket, new IPEndPoint(_context.Connection.RemotePort, _context.Connection.RemotePort), properties);
        }

        protected override void AfterAccept(Session session)
        {
            _completionSource.SetResult((WebSession)session);
        }
        public Task<WebSession> WaitOpen => _completionSource.Task;
        public override async void Reject(ArraySegment<byte> payload = default)
        {
            try
            {
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, payload.Array != null ? Convert.ToBase64String(payload.Array, payload.Offset, payload.Count) : string.Empty, CancellationToken.None);
            }
            finally
            {
                _webSocket.Dispose();
                _completionSource.SetCanceled();
            }
            
            
        }
    }
}