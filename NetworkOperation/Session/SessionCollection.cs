using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkOperation
{
    public abstract class SessionCollection : ISessionEvents, IReadOnlyCollection<Session>
    {
        public abstract Session GetSession(long id);
        protected internal abstract Task SendToAllAsync(ArraySegment<byte> data);
        
        protected void RaiseClosed(Session session)
        {
            SessionClosed?.Invoke(session);
        }

        protected void RaiseOpened(Session session)
        {
            SessionOpened?.Invoke(session);
        }

        protected void RaiseError(Session session, EndPoint endPoint, SocketError code)
        {
            SessionError?.Invoke(session, endPoint, code);
        }

        public event Action<Session> SessionClosed;
        public event Action<Session> SessionOpened;
        public event Action<Session, EndPoint, SocketError> SessionError;
        public abstract IEnumerator<Session> GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public abstract int Count { get; }
    }

    public abstract class MutableSessionCollection : SessionCollection, ICollection<Session>
    {
        private readonly ConcurrentDictionary<long, Session> _idToSessions = new ConcurrentDictionary<long, Session>();
        private int _fastCount;
        public override Session GetSession(long id)
        {
            _idToSessions.TryGetValue(id, out var session);
            return session;
        }

        protected internal override async Task SendToAllAsync(ArraySegment<byte> data)
        {
            foreach (var session in _idToSessions)
            {
                await session.Value.SendMessageAsync(data);
            }
        }

        public override IEnumerator<Session> GetEnumerator()
        {
            return _idToSessions.Select(pair => pair.Value).GetEnumerator();
        }

        public void OpenSession(Session session)
        {
            RaiseOpened(session);
        }
        
        public void Add(Session item)
        {
            if (_idToSessions.TryAdd(item.Id, item))
            {
                Interlocked.Increment(ref _fastCount);
            }
        }

        public override int Count => _fastCount;

        public void Clear()
        {
            try
            {
                foreach (var session in _idToSessions)
                {
                    try
                    {
                        RaiseClosed(session.Value);
                    }
                    finally
                    {
                    
                        session.Value.OnClosingSession();
                    }
                }
            }
            finally
            {
                _idToSessions.Clear();
                Interlocked.Exchange(ref _fastCount, 0);
            }
        }

        public bool Contains(Session item)
        {
            return _idToSessions.ContainsKey(item.Id);
        }

        public void CopyTo(Session[] array, int arrayIndex)
        {
            _idToSessions.Values.CopyTo(array, arrayIndex);
        }

        public bool Remove(Session item)
        {
            var removed = _idToSessions.TryRemove(item.Id, out _);
            if (removed)
            {
                try
                {
                    RaiseClosed(item);
                }
                finally
                {
                    Interlocked.Decrement(ref _fastCount);
                    item.OnClosingSession();
                }
            }
            return removed;
        }

        public void DoError(Session item, EndPoint endPoint, SocketError code)
        {
            RaiseError(item, endPoint, code);
        }
        public bool IsReadOnly => false;

    }
}