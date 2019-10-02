using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkOperation
{
    public abstract class Session
    {
        protected Session(IEnumerable<SessionProperty> properties)
        {
            _propertyContainer = new ConcurrentDictionary<string, object>(properties.Select(p => new KeyValuePair<string, object>(p.Name,p.Value)));
        }
        
        public object this[string paramName]
        {
            get
            {
                _propertyContainer.TryGetValue(paramName, out var p);
                return p;
            }
            set
            {
                _propertyContainer.AddOrUpdate(paramName, value, (s, o) => o);
            }
        }

        public IEnumerable<SessionProperty> Properties => _propertyContainer.Select(pair => new SessionProperty(pair.Key, pair.Value));

        private readonly ConcurrentDictionary<string, object> _propertyContainer;
        
        internal ICollection<Session> SessionCollection { get; set; }
        public abstract EndPoint NetworkAddress { get; }
        public abstract object UntypedConnection { get; }
        public abstract long Id { get; }
        public abstract SessionStatistics Statistics { get; }
        
        public void Close(ArraySegment<byte> payload = default)
        {
            try
            {
                if (State == SessionState.Opened) SendClosingPayload(payload);
            }
            finally
            {
                OnClosingSession();
                SessionCollection?.Remove(this);
                SessionCollection = null;
            }
        }
        internal void OnClosingSession()
        {
            if (_closing) return;
            _closing = true;
            _propertyContainer.Clear();
            OnClosedSession();
        }

        private bool _closing;
        protected abstract void OnClosedSession();
        protected abstract void SendClosingPayload(ArraySegment<byte> payload);

        public abstract SessionState State { get; }

        protected internal abstract bool HasAvailableData { get; }

        protected internal abstract Task SendMessageAsync(ArraySegment<byte> data);
        protected internal abstract Task<ArraySegment<byte>> ReceiveMessageAsync();

    }
}