using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using LiteNetLib;
using LiteNetLib.Utils;
using NetworkOperation;
using NetworkOperation.Extensions;

namespace NetLibOperation
{
    public class NetLibSession : Session
    {

        private readonly NetPeer _peer;

        public NetLibSession(NetPeer peer, IEnumerable<SessionProperty> properties) : base(properties)
        {
            _peer = peer;
        }

        public override EndPoint NetworkAddress => _peer.EndPoint;
        public override object UntypedConnection => _peer;
        public override long Id => _peer.Id;
        public override SessionStatistics Statistics { get; }

        protected override bool HasAvailableData => _hasData;

        private bool _hasData;
        
        protected override Task SendMessageAsync(ArraySegment<byte> data, DeliveryMode mode)
        {
            var delivery = mode.Convert();
            if (data.Count > _peer.GetMaxSinglePacketSize(delivery))
            {
                _peer.Send(data.Array, data.Offset, data.Count, DeliveryMethod.ReliableOrdered);
            }
            else
            {
                _peer.Send(data.Array, data.Offset, data.Count, delivery);
            }
            
            return Task.CompletedTask;
        }

        private ArraySegment<byte> _data;

        public void OnReceiveData(ArraySegment<byte> data)
        {
            _hasData = true;
            _data = data;
        }

        protected override Task<ArraySegment<byte>> ReceiveMessageAsync()
        {
            _hasData = false;
            var copy = _data;
            _data = default;
            return Task.FromResult(copy);
        }
        protected override void SendClose(ArraySegment<byte> payload)
        {
            if (payload.Array != null)
            {
                _peer.Disconnect(NetDataWriter.FromBytes(payload.Array,payload.Offset,payload.Count));
                return;
            }
            _peer.Disconnect();
        }

        public override SessionState State => DecodeState(_peer.ConnectionState);

        public static SessionState DecodeState(ConnectionState connectionState)
        {
            if ((connectionState & ConnectionState.Incoming) != 0)
            {
                return SessionState.Opening;
            }

            if ((connectionState & ConnectionState.Connected) != 0)
            {
                return SessionState.Opened;
            }

            if ((connectionState & ConnectionState.Disconnected) != 0)
            {
                return SessionState.Closed;
            }

            if ((connectionState & ConnectionState.ShutdownRequested) != 0)
            {
                return SessionState.Closed;
            }
            return SessionState.Unknown;
        }
    }
}