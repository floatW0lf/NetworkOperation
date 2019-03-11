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
        
        private CancellationTokenSource _cts = new CancellationTokenSource();

        public override void Connect(string address, int port)
        {
            PreConnect(address, port);
            _connectTask.Wait();
        }

        private void StartPoll()
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
            PreConnect(address, port);
            return _connectTask;
        }

        private void PreConnect(string address, int port)
        {
            StartPoll();
            Manager.Connect(address, port);
            _connectTask = new Task(() => {}, TaskCreationOptions.PreferFairness);
        }

        public override void Disconnect()
        {
            if (_pollingTask == null) return;
            Manager.Stop();
            _cts.Cancel();
            _pollingTask.Wait();
            _pollingTask = null;
        }

        public override async Task DisconnectAsync()
        {
            if (_pollingTask == null) return;

            await Task.Run(() =>
            {
                Manager.Stop();
                while (Manager.IsRunning) { }
            });
            _cts.Cancel();
            await _pollingTask;
            _pollingTask = null;
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
            OpenSession(peer);
            _connectTask.Start();
        }

        void INetEventListener.OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
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
