using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetworkOperation
{
    public abstract class SessionCollection : IReadOnlyList<Session>, ISessionEvents
    {
        protected readonly ConcurrentDictionary<long, Session> IdToSessions = new ConcurrentDictionary<long, Session>();
        protected readonly List<Session> Session = new List<Session>();

        public virtual Session GetSession(long id)
        {
            IdToSessions.TryGetValue(id, out var session);
            return session;
        }

        protected internal virtual async Task SendToAllAsync(byte[] data)
        {
            for (int i = 0; i < Session.Count; i++)
            {
                await Session[i].SendMessageAsync(new ArraySegment<byte>(data));
            }
        }

        public virtual IEnumerator<Session> GetEnumerator()
        {
            return IdToSessions.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => Session.Count;
        public Session this[int index] => Session[index];

        protected void RaiseClosed(Session session)
        {
            OnSessionClosed?.Invoke(session);
        }

        protected void RaiseOpened(Session session)
        {
            OnSessionOpened?.Invoke(session);
        }

        protected void RaiseError(Session session, string errorMessage, int code)
        {
            OnSessionError?.Invoke(session, errorMessage, code);
        }

        public event Action<Session> OnSessionClosed;
        public event Action<Session> OnSessionOpened;
        public event Action<Session, string, int> OnSessionError;
    }

    public abstract class MutableSessionCollection : SessionCollection, ICollection<Session>
    {
        public void Add(Session item)
        {
            if (IdToSessions.TryAdd(item.Id, item))
            {
                item.SessionCollection = this;
                Session.Add(item);
                RaiseOpened(item);
            }
        }

        public void Clear()
        {
            for (int i = 0; i < Session.Count; i++)
            {
                var session = Session[i];
                session.OnClosedSession();
                RaiseClosed(session);
            }
            IdToSessions.Clear();
            Session.Clear();
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
                Session.Remove(item);
                item.OnClosedSession();
                RaiseClosed(item);
            }
            return removed;
        }

        public void DoError(Session item, string message, int code)
        {
            IdToSessions.TryRemove(item.Id, out _);
            Session.Remove(item);
            item.OnClosedSession();
            RaiseError(item, message, code);
        }

        public new int Count => IdToSessions.Count;
        public bool IsReadOnly => false;

    }
}