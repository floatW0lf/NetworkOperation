using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NetworkOperation.Factories;
using NetworkOperation.Logger;
using NetworkOperation.Server;

namespace NetworkOperation.Host
{
    public abstract class AbstractHost<TRequest,TResponse, TConnectionCollection> : IHostedService, IHostContext where TRequest : IOperationMessage, new() where TResponse : IOperationMessage, new()
    {
        private readonly IFactory<TConnectionCollection, MutableSessionCollection> _sessionsFactory;
        
        private readonly IFactory<MutableSessionCollection, IHostOperationExecutor> _executorFactory;
        private readonly SessionRequestHandler _handler;

        protected IStructuralLogger Logger { get; }
        public int PollTimeInMs { get; set; } = 10;
        
        protected AbstractHost(IFactory<TConnectionCollection, MutableSessionCollection> sessionsFactory,
            IFactory<SessionCollection, IHostOperationExecutor> executorFactory,
            BaseDispatcher<TRequest, TResponse> dispatcher, SessionRequestHandler handler, ILoggerFactory loggerFactory)
        {
            Logger = loggerFactory.Create(GetType().FullName);
            _sessionsFactory = sessionsFactory;
            _executorFactory = executorFactory;
            _handler = handler;
            Dispatcher = dispatcher;
            Dispatcher.ExecutionSide = Side.Server;
        }
        protected BaseDispatcher<TRequest,TResponse> Dispatcher { get; }
        private MutableSessionCollection _mutableSessions;

        
        protected void SessionOpen(Session session)
        {
            _mutableSessions.OpenSession(session);
        }

        protected void SessionClose(Session session)
        {
            _mutableSessions.Remove(session);
        }

        protected void ServerStarted(TConnectionCollection connectionCollection)
        {
            _mutableSessions = _sessionsFactory.Create(connectionCollection);
            Executor = _executorFactory.Create(_mutableSessions);
            Dispatcher.Subscribe((IResponseReceiver<TResponse>) Executor);
        }

        protected void BeforeSessionOpen(SessionRequest sessionRequest)
        {
            sessionRequest.SetupRequest(_mutableSessions);
            _handler.Handle(sessionRequest);
        }


        protected void CloseAllSession()
        {
            _mutableSessions.Clear();
        }

        protected void DoError(Session session, EndPoint endPoint, SocketError error)
        {
            _mutableSessions.DoError(session, endPoint, error);
        }
        
        public IHostOperationExecutor Executor { get; private set; }
        public SessionCollection Sessions => _mutableSessions;
        public abstract Task StartAsync(CancellationToken cancellationToken);
        public abstract Task StopAsync(CancellationToken cancellationToken);
    }
}