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
        private readonly WebSocket _webSocket;
        private ArraySegment<byte> _buffer;
        
        public WebSession(WebSocket webSocket, IEnumerable<SessionProperty> properties, int bufferSize = 8096) : base(properties)
        {
            _webSocket = webSocket;
            _buffer = ArrayPool<byte>.Shared.Rent(bufferSize).To();
        }

        public override EndPoint NetworkAddress => new IPEndPoint(0, 0);
        public override object UntypedConnection => _webSocket;
        public override long Id => _webSocket.GetHashCode();
        public override NetworkStatistics Statistics => throw new NotImplementedException();
        protected override void SendClose(ArraySegment<byte> payload)
        {
            _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, Convert.ToBase64String(payload.Array, payload.Offset, payload.Count), CancellationToken.None).GetAwaiter();
        }

        public override SessionState State
        {
            get
            {
                switch (_webSocket.State)
                {
                    case WebSocketState.Aborted:
                    case WebSocketState.Closed:
                        return SessionState.Closed;
                    case WebSocketState.CloseReceived:
                    case WebSocketState.CloseSent:
                        return SessionState.Unknown;
                    case WebSocketState.Connecting:
                        return SessionState.Opening;
                    case WebSocketState.None:
                        return SessionState.Unknown;
                    case WebSocketState.Open:
                        return SessionState.Opened;
                    default:
                        return SessionState.Unknown;
                }
            }
        }

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
            var prefixSize = -1;
            WebSocketReceiveResult result;
            do
            {
                result = await _webSocket.ReceiveAsync(_buffer, CancellationToken.None);
                if (prefixSize == -1)
                {
                    if (result.Count == 0) return default;
                    prefixSize = BitConverter.ToInt32(_buffer.Array, 0);
                    TryResize(ref _buffer, prefixSize);
                }
                _buffer = _buffer.Slice(result.Count);

            } while (!result.EndOfMessage);

            return _buffer.Slice(sizeof(int), prefixSize);
        }

        private static bool TryResize(ref ArraySegment<byte> segment, int newSize)
        {
            if (segment.Array.Length >= newSize) return false;
            var newArray = ArrayPool<byte>.Shared.Rent(newSize);
            Buffer.BlockCopy(segment.Array, 0, newArray, 0,segment.Array.Length);
            ArrayPool<byte>.Shared.Return(segment.Array);
            segment = new ArraySegment<byte>(newArray, segment.Offset, newSize);
            return true;
        }
    }
}

