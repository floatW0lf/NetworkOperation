using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using NetworkOperation.Core;
using NetworkOperation.Core.Models;

namespace Tcp.Core
{
    public class TcpSession : Session
    {
        private readonly byte[] _buffer = new byte[4092];
        private Memory<byte> _bufferControl;
        
        private readonly Socket _client;
        int _readBytes;

        public TcpSession(Socket client, IEnumerable<SessionProperty> properties) : base(properties)
        {
            _bufferControl = new Memory<byte>(_buffer);
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

        protected override void SendClose(Span<byte> payload)
        {
            
        }

        public override SessionState State =>  _client.IsConnected() ? SessionState.Opened : SessionState.Closed;

        protected override bool HasAvailableData => _client.Connected && (_client.Available > 0 || _readBytes > 0);

        protected override async Task<ReadOnlyMemory<byte>> ReceiveMessageAsync()
        {
            if (_readBytes == 0)
            {
                _bufferControl = _buffer;
            }

            var count = MemoryMarshal.Read<int>(_bufferControl.Span);
            _bufferControl = _bufferControl.Slice(sizeof(int), count);
            var frame = _bufferControl;

            if (_bufferControl.IsEmpty) _readBytes = 0;
            return frame;

        }

        protected override async Task SendMessageAsync(ReadOnlyMemory<byte> data, DeliveryMode m)
        {
            var prefix = BitConverter.GetBytes(data.Length);
        }
    }
}