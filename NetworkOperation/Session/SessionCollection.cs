using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NetworkOperation
{
    public abstract class SessionCollection : IEnumerable<Session>, ISessionEvents
    {
        protected readonly ConcurrentDictionary<long, Session> IdToSessions = new ConcurrentDictionary<long, Session>();

        public virtual Session GetSession(long id)
        {
            IdToSessions.TryGetValue(id, out var session);
            return session;
        }

        protected internal virtual async Task SendToAllAsync(ArraySegment<byte> data)
        {
            foreach (var session in IdToSessions)
            {
                await session.Value.SendMessageAsync(data);
            }
        }

        public virtual IEnumerator<Session> GetEnumerator()
        {
            return IdToSessions.Select(pair => pair.Value).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => IdToSessions.Count();
        
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
    }

    public abstract class MutableSessionCollection : SessionCollection, ICollection<Session>
    {
        public void OpenSession(Session session)
        {
            RaiseOpened(session);
        }
        
        public void Add(Session item)
        {
            if (IdToSessions.TryAdd(item.Id, item))
            {
                item.SessionCollection = this;
            }
        }

        public void Clear()
        {
            foreach (var session in IdToSessions)
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
            
            IdToSessions.Clear();
        }

        public bool Contains(Session item)
        {
            return IdToSessions.ContainsKey(item.Id);
        }

        public void CopyTo(Session[] array, int arrayIndex)
        {
            IdToSessions.Values.CopyTo(array, arrayIndex);
        }

        public bool Remove(Session item)
        {
            var removed = IdToSessions.TryRemove(item.Id, out _);
            if (removed)
            {
                try
                {
                    RaiseClosed(item);
                }
                finally
                {
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