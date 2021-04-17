using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using LiteNetLib;
using LiteNetLib.Utils;
using NetworkOperation.Core;
using NetworkOperation.Core.Models;
using NetworkOperation.LiteNet;

namespace NetLibOperation
{
    internal class NetLibSession : Session
    {
        private readonly NetPeer _peer;
        private readonly ConcurrentQueue<ArraySegment<byte>> _queue = new ConcurrentQueue<ArraySegment<byte>>();
        public NetLibSession(NetPeer peer, IEnumerable<SessionProperty> properties) : base(properties)
        {
            _peer = peer;
            Statistics = new LiteNetStatistics(peer.Statistics);
        }

        public override EndPoint NetworkAddress => _peer.EndPoint;
        public override object UntypedConnection => _peer;
        public override long Id => _peer.Id;
        public override NetworkStatistics Statistics { get; }

        protected override bool HasAvailableData => !_queue.IsEmpty;
        
        protected override Task SendMessageAsync(ArraySegment<byte> data, DeliveryMode mode)
        {
            var delivery = mode.Convert();
            if ((mode & DeliveryMode.Reliable) != DeliveryMode.Reliable && data.Count > _peer.GetMaxSinglePacketSize(delivery))
            {
                _peer.Send(data.Array, data.Offset, data.Count, DeliveryMethod.ReliableOrdered);
            }
            else
            {
                _peer.Send(data.Array, data.Offset, data.Count, delivery);
            }
            
            return Task.CompletedTask;
        }

        public void OnReceiveData(ArraySegment<byte> data)
        {
            _queue.Enqueue(data);
        }

        protected override Task<ArraySegment<byte>> ReceiveMessageAsync()
        {
            _queue.TryDequeue(out var data);
            return Task.FromResult(data);
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