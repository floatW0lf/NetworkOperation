using LiteNetLib;
using NetLibOperation;
using NetworkOperation.Core;
using Xunit;

namespace NetOperationTest
{
    public class NetLibSessionTest
    {
        [Fact]
        public void decode_state()
        {
            Assert.Equal(SessionState.Opening, NetLibSession.DecodeState(ConnectionState.Outgoing));
            Assert.Equal(SessionState.Opened, NetLibSession.DecodeState(ConnectionState.Connected | ConnectionState.Disconnected));
            Assert.Equal(SessionState.Closed, NetLibSession.DecodeState(ConnectionState.Disconnected));
        }
    }
}