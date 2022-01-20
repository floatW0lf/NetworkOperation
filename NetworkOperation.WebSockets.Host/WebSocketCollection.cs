using System;
using NetworkOperation.Core;

namespace NetworkOperation.WebSockets.Host
{
    public class WebSocketCollection : MutableSessionCollection
    {
        public override NetworkStatistics Statistics => throw new NotImplementedException();
    }
}