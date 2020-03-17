using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using NetworkOperation.Core;
using NetworkOperation.Core.Models;

namespace Tcp.Core
{
    public class TcpSession : Session
    {
        private ArraySegment<byte> _segmentedBuffer = new ArraySegment<byte>(new byte[4092]);
        private readonly Socket _client;
        int _readBytes;

        public TcpSession(Socket client, IEnumerable<SessionProperty> properties) : base(properties)
        {
            _client = client;
        }

        public override EndPoint NetworkAddress => _client.RemoteEndPoint;

        public override object UntypedConnection => _client;

        public override long Id => _client.GetHashCode();

        public override NetworkStatistics Statistics => throw new NotImplementedException();

        protected override void OnClosedSession()
        {
            if (_client.Connected) _client.Close();
        }

        protected override void SendClose(ArraySegment<byte> payload)
        {
            
        }

        public override SessionState State =>  _client.IsConnected() ? SessionState.Opened : SessionState.Closed;

        protected override bool HasAvailableData => _client.Connected && (_client.Available > 0 || _readBytes > 0);

        protected override async Task<ArraySegment<byte>> ReceiveMessageAsync()
        {
            if (_readBytes == 0)
            {
                _segmentedBuffer = _segmentedBuffer.Reset();
                _readBytes = await _client.ReceiveAsync(_segmentedBuffer, SocketFlags.None);
            }
            
            var count = BitConverter.ToInt32(_segmentedBuffer.Array, _segmentedBuffer.Offset);
            _segmentedBuffer = _segmentedBuffer.NewSegment(_segmentedBuffer.Offset + sizeof(int), count);

            var frame = _segmentedBuffer;

            var newOffset = _segmentedBuffer.Offset + count;
            _segmentedBuffer = _segmentedBuffer.NewSegment(newOffset, _readBytes - newOffset);

            if (_segmentedBuffer.Count == 0) _readBytes = 0;
            return frame;

        }

        protected override async Task SendMessageAsync(ArraySegment<byte> data, DeliveryMode m)
        {
            var prefix = BitConverter.GetBytes(data.Count);
            await _client.SendAsync(new[] {prefix.To(), data}, SocketFlags.None);
        }
    }
}