using System;
using System.Collections.Generic;
using LiteNetLib;
using NetLibOperation;
using NetworkOperation.Core;
using NetworkOperation.Host;

namespace NetworkOperation.LiteNet.Host
{
    public class LiteSessionRequest : SessionRequest
    {
        private readonly ConnectionRequest _connectionRequest;

        public LiteSessionRequest(ConnectionRequest connectionRequest)
        {
            _connectionRequest = connectionRequest;
        }
        
        public override ReadOnlyMemory<byte> RequestPayload => new ArraySegment<byte>(_connectionRequest.Data.RawData,_connectionRequest.Data.UserDataOffset,_connectionRequest.Data.UserDataSize);
        
        protected override Session Accepted(IEnumerable<SessionProperty> properties)
        {
            return new NetLibSession(_connectionRequest.Accept(), properties);
        }

        public override void Reject(ReadOnlyMemory<byte> payload = default)
        {
            if (payload.IsEmpty)
            {
                _connectionRequest.Reject();
                return;
            }
            _connectionRequest.Reject(payload.ToArray());
        }
    }
}