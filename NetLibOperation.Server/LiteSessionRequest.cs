using System;
using LiteNetLib;
using NetworkOperation;
using NetworkOperation.Factories;
using NetworkOperation.Host;

namespace NetLibOperation
{
    public class LiteSessionRequest : SessionRequest
    {
        private readonly ConnectionRequest _connectionRequest;
        public IFactory<NetPeer, Session> SessionFactory { get; set; }

        public LiteSessionRequest(ConnectionRequest connectionRequest)
        {
            _connectionRequest = connectionRequest;
        }
        
        public override ArraySegment<byte> RequestPayload => new ArraySegment<byte>(_connectionRequest.Data.RawData,_connectionRequest.Data.UserDataOffset,_connectionRequest.Data.UserDataSize);
        
        protected override Session Accepted()
        {
            return SessionFactory.Create(_connectionRequest.Accept());
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