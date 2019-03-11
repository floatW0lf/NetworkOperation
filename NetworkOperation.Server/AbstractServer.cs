using System;
using System.Threading.Tasks;
using NetworkOperation.Factories;

namespace NetworkOperation.Server
{
    public abstract class AbstractServer<TRequest,TResponse, TConnection, TConnectionCollection> : IServer where TRequest : IOperationMessage, new() where TResponse : IOperationMessage, new()
    {
        private readonly IFactory<TConnectionCollection, MutableSessionCollection> _sessionsFactory;
        private readonly IFactory<TConnection, Session> _sessionFactory;
        private readonly IFactory<MutableSessionCollection, IServerOperationExecutor> _executorFactory;

        public int PollTimeInMs { get; set; } = 10;
        protected AbstractServer(
            IFactory<TConnectionCollection,MutableSessionCollection> sessionsFactory, 
            IFactory<TConnection,Session> sessionFactory,
            IFactory<SessionCollection,IServerOperationExecutor> executorFactory,
            BaseDispatcher<TRequest,TResponse> dispatcher)
        {
            _sessionsFactory = sessionsFactory;
            _sessionFactory = sessionFactory;
            _executorFactory = executorFactory;
            Dispatcher = dispatcher;
            Dispatcher.ExecutionSide = Side.Server;
        }
        protected BaseDispatcher<TRequest,TResponse> Dispatcher { get; }
        private MutableSessionCollection _mutableSessions;

        
        protected void SessionOpen(TConnection connection)
        {
            _mutableSessions.Add(_sessionFactory.Create(connection));
        }

        protected void ServerStarted(TConnectionCollection connectionCollection)
        {
            _mutableSessions = _sessionsFactory.Create(connectionCollection);
            Executor = _executorFactory.Create(_mutableSessions);
            Dispatcher.Subscribe((IResponseReceiver<TRequest>) Executor);
        }


        protected void CloseAllSession()
        {
            _mutableSessions.Clear();
        }
        
        public IServerOperationExecutor Executor { get; private set; }
        public abstract void Start(int port);
        public abstract void Shutdown();
        public SessionCollection Sessions => _mutableSessions;
        public abstract Task ShutdownAsync();
        
    }
}