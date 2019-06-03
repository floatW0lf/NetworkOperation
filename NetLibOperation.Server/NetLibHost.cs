using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using LiteNetLib;
using NetworkOperation;
using NetworkOperation.Factories;
using NetworkOperation.Host;

namespace NetLibOperation
{
    public class NetLibHost<TMessage, TResponse> : AbstractHost<TMessage, TResponse, NetManager>,
        INetEventListener where TMessage : IOperationMessage, new() where TResponse : IOperationMessage, new()
    {
        
        private Task _pollTask;

        private readonly CancellationTokenSource _source = new CancellationTokenSource();

        public NetLibHost(IFactory<NetManager, MutableSessionCollection> sessionsFactory,
            IFactory<SessionCollection, IHostOperationExecutor> executorFactory,
            BaseDispatcher<TMessage, TResponse> dispatcher, 
            SessionRequestHandler handler) : base(sessionsFactory,executorFactory, dispatcher,handler)
        {
           

            Manager = new NetManager(this);
        }

        public NetManager Manager { get; }

        public int MaxConnection { get; set; } = 10000;
        void INetEventListener.OnPeerConnected(NetPeer peer)
        {
            SessionOpen(Sessions.GetSession(peer.Id));
        }

        void INetEventListener.OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            var session = Sessions.GetSession(peer.Id);
            if (session != null)
            {
                session.FillDisconnectInfo(disconnectInfo);
                session.Close();
            }
            
        }

        void INetEventListener.OnNetworkError(IPEndPoint endPoint, SocketError socketError)
        {
            var session = Sessions.FirstOrDefault(s => Equals(s.NetworkAddress, endPoint));
            DoError(session, endPoint, socketError);
        }

        void INetEventListener.OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            var session = Sessions.GetSession(peer.Id);
            ((NetLibSession) session).OnReceiveData(new ArraySegment<byte>(reader.RawData, reader.UserDataOffset,
                reader.UserDataSize));
            Dispatcher.DispatchAsync(session).GetAwaiter();
        }

        void INetEventListener.OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader,
            UnconnectedMessageType messageType)
        {
        }

        void INetEventListener.OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
        }

        public void OnConnectionRequest(ConnectionRequest request)
        {
            BeforeSessionOpen(new LiteSessionRequest(request));
        }

        public override void Start(int port)
        {
            if (Manager.Start(port))
            {
                _pollTask = Task.Factory.StartNew(async () =>
                {
                    do
                    {
                        Manager.PollEvents();
                        await Task.Delay(PollTimeInMs);
                        
                    } while (!_source.Token.IsCancellationRequested);
                    
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
                while (Manager.IsRunning)
                {
                }
            });
            _source.Cancel();
            _pollTask = null;
        }

        ~NetLibHost()
        {
            Shutdown();
        }
    }
}