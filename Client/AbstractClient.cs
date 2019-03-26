using System;
using System.Net;
using System.Net.Sockets;
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
        }

        private IFactory<TConnection,Session> _sessionFactory;
        private IFactory<Session,IClientOperationExecutor> _executorFactory;
        private readonly BaseDispatcher<TRequest,TResponse> _dispatcher;

        public IClientOperationExecutor Executor { get; private set; }
        public abstract void Connect(string address, int port);
        public abstract Task ConnectAsync(string address, int port);
        public abstract void Disconnect();
        public abstract Task DisconnectAsync();
        
        protected void CloseSession()    
        {
            Session.Close();
            OnSessionClosed?.Invoke(Session);
            
            OnSessionClosed = null;
            OnSessionError = null;
            OnSessionOpened = null;
        }

        protected Session Session { get; private set; }
        
        protected void OpenSession(TConnection session)
        {
            Session = _sessionFactory.Create(session);
            Executor = _executorFactory.Create(Session);
            _dispatcher.Subscribe((IResponseReceiver<TRequest>) Executor);
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
    }
}