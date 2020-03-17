using System;
using System.Net;
using System.Net.Sockets;

namespace NetworkOperation.Core
{
    public interface ISessionEvents
    {
        event Action<Session> SessionClosed;
        event Action<Session> SessionOpened;
        event Action<Session, EndPoint, SocketError> SessionError;
    }
}