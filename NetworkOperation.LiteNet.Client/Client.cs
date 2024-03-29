﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using LiteNetLib;
using LiteNetLib.Utils;
using Microsoft.Extensions.Logging;
using NetLibOperation;
using NetworkOperation.Client;
using NetworkOperation.Core;
using NetworkOperation.Core.Dispatching;
using NetworkOperation.Core.Factories;
using NetworkOperation.Core.Messages;

namespace NetworkOperation.LiteNet.Client
{
    public class Client<TRequest, TResponse> : AbstractClient<TRequest, TResponse, NetPeer>, INetEventListener
        where TRequest : IOperationMessage, new() where TResponse : IOperationMessage, new()
    {
        private bool _eventLoopRun;

        private CancellationTokenSource _globalCancellationTokenSource;

        private Task _pollingTask;
        private TaskCompletionSource<byte> _connectSource;
        
        public Client(IFactory<NetPeer, Session> sessionFactory, IFactory<Session, IClientOperationExecutor> executorFactory, BaseDispatcher<TRequest, TResponse> dispatcher, BaseSerializer serializer, ILoggerFactory loggerFactory) : base(sessionFactory, executorFactory, dispatcher, serializer, loggerFactory)
        {
            Manager = new NetManager(this);
        }

        public NetManager Manager { get; private set; }
        public TimeSpan ConnectTimeOut { get; set; } = TimeSpan.FromSeconds(5);
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
            try
            {
                OpenSession(peer);
            }
            finally
            {
                ((IGlobalCancellation) Executor).GlobalToken = _globalCancellationTokenSource.Token;
                _connectSource.SetResult(0);
            }
        }

        void INetEventListener.OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            GlobalCancel();
            Session.FillDisconnectInfo(disconnectInfo);
            CloseSession();
        }
       

        void INetEventListener.OnNetworkError(IPEndPoint endPoint, SocketError socketError)
        {
            GlobalCancel();
            DoErrorSession(endPoint,socketError);
        }

        void INetEventListener.OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            ((NetLibSession) Session).OnReceiveData(new ArraySegment<byte>(reader.RawData,reader.UserDataOffset,reader.UserDataSize));
            Dispatch().ConfigureAwait(false).GetAwaiter();
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
                    try
                    {
                        Manager.NatPunchModule.PollEvents();
                        Manager.PollEvents();
                    }
                    catch (Exception e)
                    {
                        Logger.LogWarning("Client event loop error {E}", e);
                    }
                    
                    await Task.Delay(PollTimeInMs).ConfigureAwait(false);
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
        private async Task InternalConnect(EndPoint remote, CancellationToken cancellationToken, NetDataWriter writer)
        {
            try
            {
                if (Session == null || Session.State == SessionState.Closed)
                {
                    _globalCancellationTokenSource = _globalCancellationTokenSource ?? new CancellationTokenSource();
                    InitEventLoop();

                    using var timeOutSource = new CancellationTokenSource(ConnectTimeOut);
                    using var compound = CancellationTokenSource.CreateLinkedTokenSource(_globalCancellationTokenSource.Token, cancellationToken, timeOutSource.Token);
                    _connectSource = new TaskCompletionSource<byte>();
                    compound.Token.Register(() => _connectSource.SetCanceled());
                    Manager.Connect((IPEndPoint) remote, writer);
                    await _connectSource.Task;
                    return;
                }
            }
            catch (OperationCanceledException)
            {
                Manager.Stop();
                throw;
            }

            Logger.LogWarning("Client already connected");
        }

        public override async Task ConnectAsync<T>(EndPoint remote, T payload, CancellationToken cancellationToken = default)
        {
            var bytes = Serializer.Serialize(payload, null);
            await InternalConnect(remote, cancellationToken, NetDataWriter.FromBytes(bytes,0,bytes.Length));
        }

        public override async Task DisconnectAsync()
        {
            if (Session?.State == SessionState.Opened)
            {
                await Task.Factory.StartNew(() => Manager.Stop());
                CloseSession();
                return;
            }
            Logger.LogWarning("Client already disconnect");
            
        }

    }
}