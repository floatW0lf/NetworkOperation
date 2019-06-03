using System;
using System.Net;
using System.Net.Sockets;

namespace NetworkOperation
{
    public interface ISessionEvents
    {
        event Action<Session> OnSessionClosed;
        event Action<Session> OnSessionOpened;
        event Action<Session, EndPoint, SocketError> OnSessionError;
    }
}