using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetworkOperation
{
    public abstract class Session
    {
        internal ICollection<Session> SessionCollection { get; set; }
        public abstract string NetworkAddress { get; }
        public abstract object UntypedConnection { get; }
        public abstract long Id { get; }
        public abstract SessionStatistics Statistics { get; }

        public void Close()
        {
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