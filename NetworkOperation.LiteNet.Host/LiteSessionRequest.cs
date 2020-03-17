using System;
using System.Collections.Generic;
using LiteNetLib;
using NetworkOperation.Core;
using NetworkOperation.Host;

namespace NetLibOperation
{
    public class LiteSessionRequest : SessionRequest
    {
        private readonly ConnectionRequest _connectionRequest;

        public LiteSessionRequest(ConnectionRequest connectionRequest)
        {
            _connectionRequest = connectionRequest;
        }
        
        public override ArraySegment<byte> RequestPayload => new ArraySegment<byte>(_connectionRequest.Data.RawData,_connectionRequest.Data.UserDataOffset,_connectionRequest.Data.UserDataSize);
        
        protected override Session Accepted(IEnumerable<SessionProperty> properties)
        {
            return new NetLibSession(_connectionRequest.Accept(), properties);
        }

        public override void Reject(ArraySegment<byte> payload = default)
        {
            if (payload.Array == null)
            {
                _connectionRequest.Reject();
                return;
            }
            _connectionRequest.Reject(payload.Array,payload.Offset,payload.Count);
        }
    }
}