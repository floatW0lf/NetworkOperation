﻿using System;
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
using NetworkOperation.Logger;

namespace NetLibOperation
{
    public class NetLibHost<TMessage, TResponse> : AbstractHost<TMessage, TResponse, NetManager>,
        INetEventListener where TMessage : IOperationMessage, new() where TResponse : IOperationMessage, new()
    {
        public int ListenPort { get; set; } = 8888;
        
        private Task _pollTask;

        private readonly CancellationTokenSource _source = new CancellationTokenSource();

        public NetLibHost(IFactory<NetManager, MutableSessionCollection> sessionsFactory,
            IFactory<SessionCollection, IHostOperationExecutor> executorFactory,
            BaseDispatcher<TMessage, TResponse> dispatcher, 
            SessionRequestHandler handler, ILoggerFactory loggerFactory) : base(sessionsFactory,executorFactory, dispatcher,handler,loggerFactory)
        {
            Manager = new NetManager(this);
        }

        public NetManager Manager { get; }
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
            Dispatcher.DispatchAsync(session).ConfigureAwait(false).GetAwaiter();
        }

        void INetEventListener.OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader,
            UnconnectedMessageType messageType)
        {
        }

        void INetEventListener.OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
        }

        void INetEventListener.OnConnectionRequest(ConnectionRequest request)
        {
            BeforeSessionOpen(new LiteSessionRequest(request));
        }

        private void Start(int port)
        {
            if (Manager.Start(port))
            {
                _pollTask = Task.Factory.StartNew(async () =>
                {
                    do
                    {
                        try
                        {
                            Manager.PollEvents();
                        }
                        catch (Exception e)
                        {
                            Logger.Write(LogLevel.Error,"Poll event thread error", e);
                        }
                        await Task.Delay(PollTimeInMs).ConfigureAwait(false);
                        
                    } while (!_source.Token.IsCancellationRequested);
                    
                }, TaskCreationOptions.LongRunning);
                ServerStarted(Manager);
            }
        }

        private void Shutdown()
        {
            if (_pollTask == null) return;
            Manager.Stop();
            _source.Cancel();
            _pollTask.Wait();
            _pollTask = null;
        }

        private async Task ShutdownAsync()
        {
            if (_pollTask == null) return;
            await Task.Run(() => Manager.Stop());
            _source.Cancel();
            await _pollTask;
            _pollTask = null;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            Start(ListenPort);
            Logger.Write(LogLevel.Info,"Network operation host started");
            return Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await ShutdownAsync();
            Logger.Write(LogLevel.Info,"Network operation host stopped");
        }
    }
}