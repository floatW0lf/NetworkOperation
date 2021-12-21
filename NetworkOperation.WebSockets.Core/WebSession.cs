using System;
using System.Buffers;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using NetworkOperation.Core;
using NetworkOperation.Core.Models;


namespace NetworkOperation.WebSockets.Core
{
    public class WebSession : Session
    {
        private  WebSocket _webSocket;
        private readonly ArraySegment<byte> _buffer;
        private WebSocketReceiveResult? _result;

        public WebSession(WebSocket webSocket, ArraySegment<byte> buffer, IEnumerable<SessionProperty> properties) : base(properties)
        {
            _webSocket = webSocket;
            _buffer = buffer;
        }

        public override EndPoint NetworkAddress => new IPEndPoint(0, 0);
        public override object UntypedConnection => _webSocket;
        public override long Id => _webSocket.GetHashCode();
        public override NetworkStatistics Statistics => throw new NotImplementedException();
        protected override void SendClose(ArraySegment<byte> payload)
        {
            _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, Convert.ToBase64String(payload), CancellationToken.None).GetAwaiter();
        }

        public override SessionState State => _webSocket.State switch
        {
            WebSocketState.Aborted => SessionState.Closed,
            WebSocketState.Closed => SessionState.Closed,
            WebSocketState.CloseReceived => SessionState.Unknown,
            WebSocketState.CloseSent => SessionState.Unknown,
            WebSocketState.Connecting => SessionState.Opening,
            WebSocketState.None => SessionState.Unknown,
            WebSocketState.Open => SessionState.Opened,
            _ => SessionState.Unknown
        };

        protected override void OnClosedSession()
        {
            ArrayPool<byte>.Shared.Return(_buffer.Array);
        }

        protected override bool HasAvailableData => _webSocket.State == WebSocketState.Open;
        protected override async Task SendMessageAsync(ArraySegment<byte> data, DeliveryMode mode)
        {
            await _webSocket.SendAsync(data, WebSocketMessageType.Binary, true, CancellationToken.None);
        }

        protected override async Task<ArraySegment<byte>> ReceiveMessageAsync()
        {
            var bufferWithPosition = _buffer;
            var result = await _webSocket.ReceiveAsync(_buffer, CancellationToken.None);
            
            while (!result.EndOfMessage)
            {
                bufferWithPosition = _buffer.Slice(result.Count);
                result = await _webSocket.ReceiveAsync(bufferWithPosition, CancellationToken.None);
            }
            return bufferWithPosition;
        }
    }
}

