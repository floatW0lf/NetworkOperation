using System;
using System.Collections.Generic;

namespace NetworkOperation.Host
{
    public abstract class SessionRequest
    {
        internal MutableSessionCollection SessionCollection { get; set; }
        public abstract ArraySegment<byte> RequestPayload { get; }

        public Session Accept(IEnumerable<SessionProperty> properties)
        {
            var session = Accepted(properties);
            SessionCollection.Add(session);
            return session;
        }

        protected abstract Session Accepted(IEnumerable<SessionProperty> properties);
        public abstract void Reject(ArraySegment<byte> payload = default);

        public SessionCollection Sessions => SessionCollection;

    }
}