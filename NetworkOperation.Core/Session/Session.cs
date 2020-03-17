using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using NetworkOperation.Core.Models;

namespace NetworkOperation.Core
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
        
        public abstract EndPoint NetworkAddress { get; }
        public abstract object UntypedConnection { get; }
        public abstract long Id { get; }
        public abstract NetworkStatistics Statistics { get; }
        
        public void Close(ArraySegment<byte> payload = default)
        {
            if (State != SessionState.Opened) return;
            SendClose(payload);
        }
        internal void OnClosingSession()
        {
            foreach (var value in _propertyContainer.Values.OfType<IDisposable>())
            {
                value.Dispose();
            }
            _propertyContainer.Clear();
            OnClosedSession();
        }

        protected virtual void OnClosedSession(){}
        protected abstract void SendClose(ArraySegment<byte> payload);

        public abstract SessionState State { get; }

        protected internal abstract bool HasAvailableData { get; }

        protected internal abstract Task SendMessageAsync(ArraySegment<byte> data, DeliveryMode mode);
        protected internal abstract Task<ArraySegment<byte>> ReceiveMessageAsync();

    }
}