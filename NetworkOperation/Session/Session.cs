using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkOperation
{
    public abstract class Session
    {
        public object this[string paramName]
        {
            get
            {
                _options.Value.TryGetValue(paramName, out var p);
                return p;
            }
            set
            {
                _options.Value.AddOrUpdate(paramName, value, (s, o) => o);
            }
        }

        private readonly Lazy<ConcurrentDictionary<string, object>> _options = new Lazy<ConcurrentDictionary<string, object>>(true);
        
        internal ICollection<Session> SessionCollection { get; set; }
        public abstract EndPoint NetworkAddress { get; }
        public abstract object UntypedConnection { get; }
        public abstract long Id { get; }
        public abstract SessionStatistics Statistics { get; }

        public void Close(ArraySegment<byte> payload = default)
        {
            if (SessionCollection != null)
            {
                SessionCollection.Remove(this);
                return;
            }

            if (_options.IsValueCreated)
            {
                _options.Value.Clear();
            }
            
            OnClosedSession(payload);
        }

        protected internal abstract void OnClosedSession(ArraySegment<byte> payload = default);

        public abstract SessionState State { get; }

        protected internal abstract bool HasAvailableData { get; }

        protected internal abstract Task SendMessageAsync(ArraySegment<byte> data);
        protected internal abstract Task<ArraySegment<byte>> ReceiveMessageAsync();

        protected internal abstract IHandler<TOp, TResult, TRequest> GetHandler<TOp,TResult,TRequest>() where TOp : IOperation<TOp, TResult> where TRequest : IOperationMessage;

    }
}