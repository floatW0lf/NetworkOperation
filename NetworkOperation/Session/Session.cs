using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace NetworkOperation
{
    public abstract class Session
    {
        private ConcurrentDictionary<string, object> _sessionParams;
        public object this[string paramName]
        {
            get
            {
                object p = null;
                CreateOrGet()?.TryGetValue(paramName, out p);
                return p;
            }
            set
            {
                CreateOrGet()?.AddOrUpdate(paramName, value, (s, o) => o);
            }
        }
        
        private ConcurrentDictionary<string, object> CreateOrGet()
        {
           return _sessionParams = _sessionParams ?? new ConcurrentDictionary<string, object>();
        }

        internal ICollection<Session> SessionCollection { get; set; }
        public abstract EndPoint NetworkAddress { get; }
        public abstract object UntypedConnection { get; }
        public abstract long Id { get; }
        public abstract SessionStatistics Statistics { get; }

        public void Close()
        {
            _sessionParams?.Clear();
            if (SessionCollection != null)
            {
                SessionCollection.Remove(this);
                return;
            }
            OnClosedSession();
        }

        protected internal abstract void OnClosedSession();

        public abstract SessionState State { get; }

        protected internal abstract bool HasAvailableData { get; }

        protected internal abstract Task SendMessageAsync(ArraySegment<byte> data);
        protected internal abstract Task<ArraySegment<byte>> ReceiveMessageAsync();

        protected internal abstract IHandler<TOp, TResult, TRequest> GetHandler<TOp,TResult,TRequest>() where TOp : IOperation<TOp, TResult> where TRequest : IOperationMessage;

    }
}