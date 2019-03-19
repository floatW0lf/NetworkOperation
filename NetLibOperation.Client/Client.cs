using System;
using System.Threading;
using System.Threading.Tasks;
using LiteNetLib;
using LiteNetLib.Utils;
using NetworkOperation;
using NetworkOperation.Client;
using NetworkOperation.Factories;

namespace NetLibOperation.Client
{
    public class Client<TRequest, TResponse> : AbstractClient<TRequest, TResponse, NetPeer>, INetEventListener,
        IDisposable where TRequest : IOperationMessage, new() where TResponse : IOperationMessage, new()
    {
        private Task _connectTask;
        private Task _disconnectTask;
        
        private bool _eventLoopRun;

        private CancellationTokenSource _globalCancellationTokenSource;

        private Task _pollingTask;

        public Client(IFactory<NetPeer, Session> sessionFactory,
            IFactory<Session, IClientOperationExecutor> executorFactory, BaseDispatcher<TRequest, TResponse> dispatcher,
            string connectKey) : base(sessionFactory, executorFactory, dispatcher)
        {
            Manager = new NetManager(this, connectKey);
        }

        public NetManager Manager { get; private set; }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Disposed();
        }

        void INetEventListener.OnNetworkError(NetEndPoint endPoint, int socketErrorCode)
        {
            DoErrorSession($"Error {endPoint}", socketErrorCode);
        }

        void INetEventListener.OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
        }

        void INetEventListener.OnNetworkReceive(NetPeer peer, NetDataReader reader)
        {
            ((NetLibSession) Session).OnReceiveData(reader.Data);
            Dispatch().GetAwaiter();
        }

        void INetEventListener.OnNetworkReceiveUnconnected(NetEndPoint remoteEndPoint, NetDataReader reader,
            UnconnectedMessageType messageType)
        {
        }

        void INetEventListener.OnPeerConnected(NetPeer peer)
        {
            try
            {
                if (_globalCancellationTokenSource == null)
                    _globalCancellationTokenSource = new CancellationTokenSource();
                OpenSession(peer);
                ((IGlobalCancellation) Executor).GlobalToken = _globalCancellationTokenSource.Token;
            }
            finally
            {
                if (_connectTask.Status >= TaskStatus.Created && _connectTask.Status < TaskStatus.Running)
                    _connectTask.Start();
            }
        }

        void INetEventListener.OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            GlobalCancel();
            CloseSession();
        }

        public override void Connect(string address, int port)
        {
            if (Session == null || Session.State == SessionState.Opened) return;
            _connectTask = PreConnect(address, port);
            _connectTask.Wait();
        }

        private void InitEventLoop()
        {
            _eventLoopRun = true;
            Manager.Start();
            if (_pollingTask != null) return;

            _pollingTask = Task.Factory.StartNew(async () =>
            {
                do
                {
                    Manager.PollEvents();
                    await Task.Delay(PollTimeInMs);
                } while (_eventLoopRun);
            }, TaskCreationOptions.LongRunning);
        }


        public override Task ConnectAsync(string address, int port)
        {
            if (Session == null || Session.State == SessionState.Closed)
            {
                _connectTask = PreConnect(address, port);
                return _connectTask;
            }

            return Task.CompletedTask;
        }

        private Task PreConnect(string address, int port)
        {
            InitEventLoop();
            Manager.Connect(address, port);
            return new Task(() => { }, TaskCreationOptions.PreferFairness);
        }

        public override void Disconnect()
        {
            if (Session?.State == SessionState.Opened)
            {
                GlobalCancel(true);
                CloseSession();
            }
        }

        private void GlobalCancel(bool stopEventLoop = false)
        {
            if (stopEventLoop)
            {
                Manager.Stop();
                _eventLoopRun = false;
            }
            
            if (_globalCancellationTokenSource != null)
            {
                _globalCancellationTokenSource.Cancel();
                _globalCancellationTokenSource.Dispose();
                _globalCancellationTokenSource = null;
            }
        }

        public override async Task DisconnectAsync()
        {
            Disconnect();
        }

        private void Disposed()
        {
            GlobalCancel(true);
            Manager = null;
        }

        ~Client()
        {
            Disposed();
        }
    }
}