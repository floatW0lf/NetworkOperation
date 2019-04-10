using System;

namespace NetworkOperation.Host
{
    public abstract class SessionRequest
    {
        internal MutableSessionCollection SessionCollection { get; set; }
        public abstract ArraySegment<byte> RequestPayload { get; }

        public Session Accept()
        {
            var session = Accepted();
            SessionCollection.Add(session);
            return session;
        }

        protected abstract Session Accepted();
        public abstract void Reject(ArraySegment<byte> payload = default);

    }
}