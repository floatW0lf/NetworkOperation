﻿using NetworkOperation.Core;

namespace Tcp.Server
{
    public class TcpSessionCollection : MutableSessionCollection
    {
        public override NetworkStatistics Statistics { get; }
    }
}