using NetworkOperation;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using NetworkOperation.Extensions;

namespace Tcp.Core
{
    public class TcpSession : Session
    {
        private ArraySegment<byte> _segmentedBuffer = new ArraySegment<byte>(new byte[4092]);
        private readonly Socket _client;
        private ConcurrentDictionary<Type, IHandler> _perSessionHandler = new ConcurrentDictionary<Type, IHandler>();
        private IHandlerFactory factory;
        int _readBytes;

        public TcpSession(Socket client, IHandlerFactory factory)
        {
            this._client = client;
            this.factory = factory;
        }

        public override EndPoint NetworkAddress => _client.RemoteEndPoint;

        public override object UntypedConnection => _client;

        public override long Id => _client.GetHashCode();

        public override SessionStatistics Statistics => throw new System.NotImplementedException();

        protected override void OnClosedSession()
        {
            if (_client.Connected) _client.Close();
            foreach (var handler in _perSessionHandler.Values)
            {
                factory.Destroy(handler);
            }
            _perSessionHandler.Clear();
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

        protected override IHandler<TOp, TResult,TRequest> GetHandler<TOp, TResult,TRequest>()
        {
            return (IHandler<TOp, TResult,TRequest>)_perSessionHandler.GetOrAdd(typeof(IHandler<TOp, TResult,TRequest>), type => factory.Create<TOp, TResult,TRequest>());
        }

        protected override async Task SendMessageAsync(ArraySegment<byte> data)
        {
            var prefix = BitConverter.GetBytes(data.Count);
            await _client.SendAsync(new[] {prefix.To(), data}, SocketFlags.None);
        }
    }
}