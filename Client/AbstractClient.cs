using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NetworkOperation.Factories;
using NetworkOperation.Logger;

namespace NetworkOperation.Client
{
    public abstract class AbstractClient<TRequest,TResponse, TConnection> : ISessionEvents, IClient where TRequest : IOperationMessage, new() where TResponse : IOperationMessage, new()
    {
        public int PollTimeInMs { get; set; } = 10;

        public IStructuralLogger Logger { get; }
        
        protected AbstractClient(IFactory<TConnection,Session> sessionFactory, IFactory<Session,IClientOperationExecutor> executorFactory, BaseDispatcher<TRequest,TResponse> dispatcher, IStructuralLogger logger)
        {
            _sessionFactory = sessionFactory;
            _executorFactory = executorFactory;
            _dispatcher = dispatcher;
            _dispatcher.ExecutionSide = Side.Client;
            Logger = logger;
            Session = new NotConnectedSession();
        }

        private IFactory<TConnection,Session> _sessionFactory;
        private IFactory<Session,IClientOperationExecutor> _executorFactory;
        private readonly BaseDispatcher<TRequest,TResponse> _dispatcher;
        private IClientOperationExecutor _executor;
        
        public IPayloadResolver ConnectionPayload { get; set; } = new NullPayloadResolver();
        public ClientState Current
        {
            get
            {
                switch (Session?.State)
                {
                    case SessionState.Opened:
                        return ClientState.Connected;
                    case SessionState.Opening:
                        return ClientState.Connecting;
                    default:
                        return ClientState.Disconnected;
                }
            }
        }

        public IClientOperationExecutor Executor
        {
            get
            {
                if (_executor == null) throw new InvalidOperationException("Client not connected");
                return _executor;
            }
        }

        public abstract Task ConnectAsync(EndPoint remote, CancellationToken cancellationToken = default);

        public abstract Task ConnectAsync<T>(EndPoint remote, T payload, CancellationToken cancellationToken = default) where T : IConnectPayload;
        

        public abstract Task DisconnectAsync();
        
        protected void CloseSession()
        {
            _executor = null;
            if (Session == null) return;
            OnSessionClosed?.Invoke(Session);
            Session.Close();
            Session = new NotConnectedSession();
        }

        protected Session Session { get; private set; }
        
        protected void OpenSession(TConnection session)
        {
            Session = _sessionFactory.Create(session);
            _executor = _executorFactory.Create(Session);
            _dispatcher.Subscribe((IResponseReceiver<TResponse>) _executor);
            OnSessionOpened?.Invoke(Session);
        }

        protected void DoErrorSession(EndPoint endPoint, SocketError code)
        {
            OnSessionError?.Invoke(Session, endPoint, code);
        }

        protected Task Dispatch()
        {
            return _dispatcher.DispatchAsync(Session);
        }
            
        public event Action<Session> OnSessionClosed;
        public event Action<Session> OnSessionOpened;
        public event Action<Session, EndPoint, SocketError> OnSessionError;
        public abstract void Dispose();
        
        class NotConnectedSession : Session
        {
            public override EndPoint NetworkAddress { get; }
            public override object UntypedConnection { get; }
            public override long Id { get; }
            public override SessionStatistics Statistics { get; }
            protected internal override void OnClosedSession(ArraySegment<byte> payload = default)
            {
                throw new NotImplementedException();
            }

            public override SessionState State { get; } = SessionState.Closed;
            protected internal override bool HasAvailableData { get; }
            protected internal override Task SendMessageAsync(ArraySegment<byte> data)
            {
                throw new NotImplementedException();
            }

            protected internal override Task<ArraySegment<byte>> ReceiveMessageAsync()
            {
                throw new NotImplementedException();
            }

            protected internal override IHandler<TOp, TResult, TRequest1> GetHandler<TOp, TResult, TRequest1>()
            {
                throw new NotImplementedException();
            }
        }
    }
}