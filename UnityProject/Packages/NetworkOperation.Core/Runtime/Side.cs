using System;

namespace NetworkOperation.Core
{
    [Flags]
    public enum Side
    {
        Client = 1,
        Server = 2,
        All = Client | Server
    }
}