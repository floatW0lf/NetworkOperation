using System;
using LiteNetLib;
using LiteNetLib.Utils;
using NetworkOperation;
using NetworkOperation.Client;
using System.Threading;
using System.Threading.Tasks;
using NetworkOperation.Factories;

namespace NetLibOperation.Client
{
    public class Client<TRequest,TResponse> : AbstractClient<TRequest,TResponse,NetPeer>, INetEventListener where TRequest : IOperationMessage, new() where TResponse : IOperationMessage, new()
    {
        public NetManager Manager { get; }
        
        private Task _pollingTask;
        private Task _connectTask;
        private Task _disconnectTask;

        private CancellationTokenSource _cts;

        public override void Connect(string address, int port)
        {
            if (Session == null || Session.State == SessionState.Opened) return;
            _connectTask = PreConnect(address, port);
            _connectTask.Wait();
        }

        private void InitEventLoop()
        {
            Manager.Start();
            _pollingTask = Task.Factory.StartNew(async() =>
            {
                do
                {
                    Manager.PollEvents();
                    await Task.Delay(PollTimeInMs);

                } while (!_cts.Token.IsCancellationRequested);

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
            _cts = new CancellationTokenSource();
            InitEventLoop();
            Manager.Connect(address, port);
            return new Task(() => {}, TaskCreationOptions.PreferFairness);
        }

        public override void Disconnect()
        {
            if (Session?.State == SessionState.Opened)
            {
                try
                {
                    Manager.Stop();
                    _cts.Cancel();
                    _pollingTask.Wait();
                }
                finally
                {
                    ClientClean();
                } 
            }
        }

        private void ClientClean()
        {
            _pollingTask = null;
            _cts?.Dispose();
            _cts = null;
        }

        public override Task DisconnectAsync()
        {
            if (Session?.State == SessionState.Opened)
            {
                Manager.Stop();
                _disconnectTask = new Task(() => {}, TaskCreationOptions.PreferFairness);
                ClientClean();
                return _disconnectTask;
            }
            
            return Task.CompletedTask;
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
            ((NetLibSession)Session).OnReceiveData(reader.Data);
            Dispatch().GetAwaiter();
        }

        void INetEventListener.OnNetworkReceiveUnconnected(NetEndPoint remoteEndPoint, NetDataReader reader, UnconnectedMessageType messageType)
        {
        }

        void INetEventListener.OnPeerConnected(NetPeer peer)
        {
            try
            {
                OpenSession(peer);
            }
            finally
            {
                if (_connectTask.Status == TaskStatus.WaitingToRun)
                {
                    _connectTask.Start();  
                }
            }
        }

        void INetEventListener.OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            _cts?.Cancel();
            _disconnectTask?.Start();
            ClientClean();
            CloseSession();
        }

        public Client(IFactory<NetPeer, Session> sessionFactory,
            IFactory<Session, IClientOperationExecutor> executorFactory, BaseDispatcher<TRequest,TResponse> dispatcher,
            string connectKey) : base(sessionFactory, executorFactory, dispatcher)
        {
            Manager = new NetManager(this, connectKey); 
        }
    }
}
