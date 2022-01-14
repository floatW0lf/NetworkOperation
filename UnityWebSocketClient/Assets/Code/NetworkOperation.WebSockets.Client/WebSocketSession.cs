using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using NetworkOperation.Core;
using NetworkOperation.Core.Models;
using WebGL.WebSockets;

namespace NetworkOperation.WebSockets.Client
{
    internal sealed class WebSocketSession : Session, IAsyncEnumerable<ArraySegment<byte>>
    {
        private WebSocket _webSocket;
        private ConcurrentQueue<ArraySegment<byte>> _queue = new ConcurrentQueue<ArraySegment<byte>>();

        public WebSocketSession(WebSocket webSocket, IEnumerable<SessionProperty> properties) : base(properties)
        {
            _webSocket = webSocket;
        }

        public void ReceiveMessage(ArraySegment<byte> data)
        {
            _queue.Enqueue(data);
        }

        protected override void SendClose(ArraySegment<byte> payload)
        {
            _webSocket.Close(WebSocketCloseCode.Normal,payload.Array != null ? Convert.ToBase64String(payload.Array, payload.Offset, payload.Count) : null);
        }


        protected override Task SendMessageAsync(ArraySegment<byte> data, DeliveryMode mode)
        {
            var buffer = new ArraySegment<byte>(ArrayPool<byte>.Shared.Rent(data.Count + sizeof(int)), 0, data.Count + sizeof(int));
            try
            {
                var size = data.Count;
                MemoryMarshal.Write(buffer, ref size);
                data.CopyTo(buffer.Slice(sizeof(int)));
                _webSocket.Send(buffer.Array, buffer.Count);
                return Task.CompletedTask;
            }
            finally
            {
                if (buffer != default)
                {
                    ArrayPool<byte>.Shared.Return(buffer.Array); 
                }
            }
            
        }

        public override EndPoint NetworkAddress { get; } = new IPEndPoint(IPAddress.Any, 0);
        public override object UntypedConnection => _webSocket;
        public override long Id => _webSocket.GetInstanceId();
        public override NetworkStatistics Statistics { get; }
        
        protected override IAsyncEnumerable<ArraySegment<byte>> Bytes => this;
        
        public override SessionState State => _webSocket.State switch
        {
            WebSocketState.Connecting => SessionState.Opening,
            WebSocketState.Open => SessionState.Opened,
            WebSocketState.Closing => SessionState.Closed,
            WebSocketState.Closed => SessionState.Closed,
            _ => SessionState.Unknown
        };
        

        public async IAsyncEnumerator<ArraySegment<byte>> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken())
        {
            while (_queue.TryDequeue(out var data))
            {
                yield return data;
            }
        }
    }
}