using System;
using System.Collections.Generic;
using NetworkOperation.Core;

namespace NetworkOperation.Host
{
    public abstract class SessionRequest
    {
        private MutableSessionCollection _sessionCollection;
        internal void SetupRequest(MutableSessionCollection value)
        {
            _sessionCollection = value;
        }

        public abstract ReadOnlyMemory<byte> RequestPayload { get; }

        public Session Accept(IEnumerable<SessionProperty> properties)
        {
            var session = Accepted(properties);
            _sessionCollection.Add(session);
            return session;
        }

        protected abstract Session Accepted(IEnumerable<SessionProperty> properties);
        public abstract void Reject(ReadOnlyMemory<byte>  payload = default);

        public SessionCollection Sessions => _sessionCollection;

    }
}