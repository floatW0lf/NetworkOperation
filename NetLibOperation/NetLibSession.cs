﻿using System;
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
        
        protected override Task SendMessageAsync(ArraySegment<byte> data)
        {
            _peer.Send(data.Array, data.Offset, data.Count, DeliveryMethod.ReliableOrdered);
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

        protected override void OnClosedSession()
        {
        }

        protected override void SendClosingPayload(ArraySegment<byte> payload)
        {
            if (payload.Array != null)
            {
                _peer.Disconnect(NetDataWriter.FromBytes(payload.Array,payload.Offset,payload.Count));
                return;
            }
            _peer.Disconnect();
        }

        public override SessionState State
        {
            get
            {
                switch (_peer.ConnectionState)
                {
                    case ConnectionState.Incoming:
                        return SessionState.Opening;
                    
                    case ConnectionState.Connected:
                        return SessionState.Opened;
                    
                    case ConnectionState.Disconnected:
                        return SessionState.Closed;
                }
                throw new ArgumentException();
            }
        }
    }
}