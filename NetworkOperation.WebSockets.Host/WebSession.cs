using System;
using System.Buffers;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using NetworkOperation.Core;
using NetworkOperation.Core.Models;


namespace NetworkOperation.WebSockets.Core
{
    internal sealed class WebSession : Session, IAsyncEnumerator<ArraySegment<byte>>, IAsyncEnumerable<ArraySegment<byte>>
    {
        private readonly WebSocket _webSocket;
        private ArraySegment<byte> _buffer;
        private CancellationToken _cancellationToken;
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
            if (_webSocket.State != WebSocketState.Open) return;
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
                    case WebSocketState.CloseReceived:
                    case WebSocketState.CloseSent:
                        return SessionState.Closed;
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
        protected override IAsyncEnumerable<ArraySegment<byte>> Bytes => this;
        protected override void OnClosedSession()
        {
            ArrayPool<byte>.Shared.Return(_buffer.Array);
        }
        protected override async Task SendMessageAsync(ArraySegment<byte> data, DeliveryMode mode)
        {
            if (_webSocket.State != WebSocketState.Open) return;
            await _webSocket.SendAsync(data, WebSocketMessageType.Binary, true, CancellationToken.None);
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

        ValueTask IAsyncDisposable.DisposeAsync()
        {
            return new ValueTask(Task.CompletedTask);
        }

        async ValueTask<bool> IAsyncEnumerator<ArraySegment<byte>>.MoveNextAsync()
        {
            WebSocketReceiveResult result;
            result = await _webSocket.ReceiveAsync(_buffer, _cancellationToken);
            if (result.Count == 0) return false;
            var prefixSize = MemoryMarshal.Read<int>(_buffer);
            TryResize(ref _buffer, prefixSize);
            _buffer = _buffer.Slice(sizeof(int)-1, prefixSize);
            while (!result.EndOfMessage)
            {
                result = await _webSocket.ReceiveAsync(_buffer, _cancellationToken);
                _buffer = _buffer.Slice(_buffer.Offset + result.Count);
            }
            _currentRawMessage = _buffer;
            _buffer = new ArraySegment<byte>(_buffer.Array);
            return true;
        }

        private ArraySegment<byte> _currentRawMessage;
        ArraySegment<byte> IAsyncEnumerator<ArraySegment<byte>>.Current => _currentRawMessage;
        public IAsyncEnumerator<ArraySegment<byte>> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken())
        {
            _cancellationToken = cancellationToken;
            return this;
        }
    }
}

