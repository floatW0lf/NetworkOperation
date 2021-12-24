using System;
using System.Buffers;
using System.Collections.Generic;
using System.Net.Sockets;
using NetworkOperation.Core;
using NetworkOperation.Host;
using Tcp.Core;

namespace Tcp.Server
{
    public class TcpSessionRequest : SessionRequest
    {
        private Socket _connection;
        public override ArraySegment<byte> RequestPayload { get; }

        public TcpSessionRequest(Socket connection, ArraySegment<byte> requestPayload)
        {
            _connection = connection;
            RequestPayload = requestPayload;
        }
        protected override Session Accepted(IEnumerable<SessionProperty> properties)
        {
            ArrayPool<byte>.Shared.Return(RequestPayload.Array);
            return new TcpSession(_connection, properties);
        }

        public override void Reject(ArraySegment<byte> payload = default)
        {
            ArrayPool<byte>.Shared.Return(RequestPayload.Array);
            _connection.SendAsync(payload, SocketFlags.None).GetAwaiter();
            _connection.Disconnect(true);
        }
    }
}