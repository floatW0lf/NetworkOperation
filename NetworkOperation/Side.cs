using System;

namespace NetworkOperation
{
    [Flags]
    public enum Side
    {
        Client = 1,
        Server = 2,
        All = Client | Server
    }
}