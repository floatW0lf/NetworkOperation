using LiteNetLib;
using LiteNetLib.Utils;
using NetworkOperation;
using NetworkOperation.Server;
using System.Threading;
using System.Threading.Tasks;
using NetworkOperation.Factories;

namespace NetLibOperation
{
    public class Server<TMessage,TResponse> : AbstractServer<TMessage,TResponse,NetPeer,NetManager>, INetEventListener where TMessage : IOperationMessage, new() where TResponse : IOperationMessage, new()
    {
        public NetManager Manager { get; }

        private CancellationTokenSource _source = new CancellationTokenSource();
        private Task _pollTask;

        public override void Start(int port)
        {
            if (Manager.Start(port))
            {
                _pollTask = Task.Factory.StartNew(() =>
                {
                    do
                    {
                        Manager.PollEvents();
                        Thread.Sleep(PollTimeInMs);
                    }
                    while (!_source.Token.IsCancellationRequested);

                }, TaskCreationOptions.LongRunning);
                ServerStarted(Manager);
            }
        }

        public override void Shutdown()
        {
            if (_pollTask == null) return;
            Manager.Stop();
            _source.Cancel();
            _pollTask.Wait();
            _pollTask = null;
        }

        public override async Task ShutdownAsync()
        {
            if (_pollTask == null) return;
            
            await Task.Run(() =>
            {
                Manager.Stop();
                while (Manager.IsRunning){}
            });
            _source.Cancel();
            _pollTask = null;
        }

        void INetEventListener.OnPeerConnected(NetPeer peer)
        {
            SessionOpen(peer);
        }

        void INetEventListener.OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            Sessions.GetSession(peer.ConnectId).Close();
        }

        void INetEventListener.OnNetworkError(NetEndPoint endPoint, int socketErrorCode)
        {
            
        }

        void INetEventListener.OnNetworkReceive(NetPeer peer, NetDataReader reader)
        {
            var session = Sessions.GetSession(peer.ConnectId);
            ((NetLibSession)session).OnReceiveData(reader.Data);
            Dispatcher.DispatchAsync(session).GetAwaiter();
        }

        void INetEventListener.OnNetworkReceiveUnconnected(NetEndPoint remoteEndPoint, NetDataReader reader, UnconnectedMessageType messageType)
        {
            
        }

        void INetEventListener.OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
            
        }

        public Server(IFactory<NetManager, MutableSessionCollection> sessionsFactory, IFactory<NetPeer, Session> sessionFactory, IFactory<SessionCollection, IServerOperationExecutor> executorFactory, BaseDispatcher<TMessage,TResponse> dispatcher, string connectionKey) : base(sessionsFactory, sessionFactory, executorFactory, dispatcher)
        {
            Manager = new NetManager(this, connectionKey);
        }
    }
}