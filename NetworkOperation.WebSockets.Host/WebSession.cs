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
using NetworkOperation.WebSockets.Host;


namespace NetworkOperation.WebSockets.Core
{
    internal sealed class WebSession : Session, IAsyncEnumerator<ArraySegment<byte>>, IAsyncEnumerable<ArraySegment<byte>>
    {
        private readonly WebSocket _webSocket;
        private ArraySegment<byte> _buffer;
        private CancellationToken _cancellationToken;
        private ArraySegment<byte> _currentRawMessage;
        public WebSession(WebSocket webSocket, IEnumerable<SessionProperty> properties, int bufferSize = 65535) : base(properties)
        {
            _webSocket = webSocket;
            _buffer = PooledArraySegment.Rent<byte>(bufferSize);
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
            PooledArraySegment.Return(_buffer);
        }
        protected override async Task SendMessageAsync(ArraySegment<byte> data, DeliveryMode mode)
        {
            if (_webSocket.State != WebSocketState.Open) return;
            await _webSocket.SendAsync(data, WebSocketMessageType.Binary, true, CancellationToken.None);
        }

        ValueTask IAsyncDisposable.DisposeAsync()
        {
            return new ValueTask(Task.CompletedTask);
        }

        async ValueTask<bool> IAsyncEnumerator<ArraySegment<byte>>.MoveNextAsync()
        {
            TryGrow(ref _buffer);
            var result = await _webSocket.ReceiveAsync(_buffer, _cancellationToken);
            if (result.Count == 0) return false;
            var messageSize = result.Count;
            _buffer = _buffer.Slice(result.Count);
            
            while (!result.EndOfMessage)
            {
                TryGrow(ref _buffer);
                result = await _webSocket.ReceiveAsync(_buffer, _cancellationToken);
                messageSize += result.Count;
                _buffer = _buffer.Slice(result.Count);
            }
            _currentRawMessage = new ArraySegment<byte>(_buffer.Array, 0, messageSize);
            _buffer = new ArraySegment<byte>(_buffer.Array);
            return true;
        }

        private void TryGrow(ref ArraySegment<byte> buffer)
        {
            const int webSocketMaxBlockSize = 16376;
            if (buffer.Count < webSocketMaxBlockSize)
            {
                PooledArraySegment.Advance(ref buffer, (buffer.Count + buffer.Offset)*2);
            }
        }

        
        ArraySegment<byte> IAsyncEnumerator<ArraySegment<byte>>.Current => _currentRawMessage;
        public IAsyncEnumerator<ArraySegment<byte>> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken())
        {
            _cancellationToken = cancellationToken;
            return this;
        }
    }
}

