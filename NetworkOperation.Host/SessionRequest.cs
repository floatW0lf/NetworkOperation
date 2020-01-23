using System;
using System.Collections.Generic;

namespace NetworkOperation.Host
{
    public abstract class SessionRequest
    {
        private MutableSessionCollection _sessionCollection;
        internal void SetupRequest(MutableSessionCollection value)
        {
            _sessionCollection = value;
        }

        public abstract ArraySegment<byte> RequestPayload { get; }

        public Session Accept(IEnumerable<SessionProperty> properties)
        {
            var session = Accepted(properties);
            _sessionCollection.Add(session);
            return session;
        }

        protected abstract Session Accepted(IEnumerable<SessionProperty> properties);
        public abstract void Reject(ArraySegment<byte> payload = default);

        public SessionCollection Sessions => _sessionCollection;

    }
}