using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using LiteNetLib;
using LiteNetLib.Utils;
using NetworkOperation;
using NetworkOperation.Client;
using NetworkOperation.Factories;
using NetworkOperation.Logger;

namespace NetLibOperation.Client
{
    public class Client<TRequest, TResponse> : AbstractClient<TRequest, TResponse, NetPeer>, INetEventListener,
        IDisposable where TRequest : IOperationMessage, new() where TResponse : IOperationMessage, new()
    {
        private readonly string _connectKey;
        private Task _connectTask;
        private Task _disconnectTask;
        
        private bool _eventLoopRun;

        private CancellationTokenSource _globalCancellationTokenSource;

        private Task _pollingTask;

        public Client(IFactory<NetPeer, Session> sessionFactory,
                      IFactory<Session, IClientOperationExecutor> executorFactory, 
                      BaseDispatcher<TRequest, TResponse> dispatcher,
                      IStructuralLogger logger,
                      string connectKey) : base(sessionFactory, executorFactory, dispatcher, logger)
        {
            _connectKey = connectKey;
            Manager = new NetManager(this);
        }

        public NetManager Manager { get; private set; }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Disposed();
        }

        public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
            
        }

        void INetEventListener.OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
        }

        void INetEventListener.OnConnectionRequest(ConnectionRequest request)
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

        void INetEventListener.OnNetworkError(IPEndPoint endPoint, SocketError socketError)
        {
            DoErrorSession(endPoint,socketError);
        }

        void INetEventListener.OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            ((NetLibSession) Session).OnReceiveData(new ArraySegment<byte>(reader.RawData,reader.UserDataOffset,reader.UserDataSize));
            Dispatch().GetAwaiter();
        }

        public override void Connect(string address, int port)
        {
            if (Session?.State == SessionState.Opened)
            {
                Logger.Write(LogLevel.Warning,"Client already connected");
                return;
            }
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
            Logger.Write(LogLevel.Warning,"Client already connected");
            return Task.CompletedTask;
        }

        private Task PreConnect(string address, int port)
        {
            InitEventLoop();
            Manager.Connect(address, port, _connectKey);
            return new Task(() => { }, TaskCreationOptions.PreferFairness);
        }

        public override void Disconnect()
        {
            if (Session?.State == SessionState.Opened)
            {
                GlobalCancel(true);
                CloseSession();
                return;
            }
            Logger.Write(LogLevel.Warning,"Client already disconnect");
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