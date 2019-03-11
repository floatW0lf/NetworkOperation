using System;

namespace NetworkOperation
{
    public interface ISessionEvents
    {
        event Action<Session> OnSessionClosed;
        event Action<Session> OnSessionOpened;
        event Action<Session, string, int> OnSessionError;
    }
}