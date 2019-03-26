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
        
        private bool _eventLoopRun;

        private CancellationTokenSource _globalCancellationTokenSource;

        private Task _pollingTask;
        private Task _connectTask;
        
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

        public override void Dispose()
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
            if (_globalCancellationTokenSource == null)
                _globalCancellationTokenSource = new CancellationTokenSource();
            
            TryStart(_connectTask);
            OpenSession(peer);
            ((IGlobalCancellation) Executor).GlobalToken = _globalCancellationTokenSource.Token;
            
        }

        private void TryStart(Task task)
        {
            if (task != null && task.Status == TaskStatus.Created)
            {
                task.Start();
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
        private void GlobalCancel(bool stopEventLoop = false)
        {
            if (stopEventLoop)
            {
                Manager.Stop();
                _eventLoopRun = false;
                _pollingTask = null;
            }
            
            if (_globalCancellationTokenSource != null)
            {
                _globalCancellationTokenSource.Cancel();
                _globalCancellationTokenSource.Dispose();
                _globalCancellationTokenSource = null;
            }
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

        public override async Task ConnectAsync(EndPoint remote, CancellationToken cancellationToken = default)
        {
            try
            {
                if (Session == null || Session.State == SessionState.Closed)
                {
                    InitEventLoop();
                    Manager.Connect((IPEndPoint)remote, _connectKey);
                    _connectTask = new Task(() => {}, cancellationToken, TaskCreationOptions.PreferFairness);
                    await _connectTask;
                    return;
                }
            }
            catch (OperationCanceledException e)
            {
                Manager.Stop();
                throw;
            }
            Logger.Write(LogLevel.Warning,"Client already connected");
        }

        public override async Task DisconnectAsync()
        {
            if (Session?.State == SessionState.Opened)
            {
                await Task.Factory.StartNew(() => Manager.Stop());
                CloseSession();
            }
            Logger.Write(LogLevel.Warning,"Client already disconnect");
            
        }
    }
}