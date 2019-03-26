using System;
using System.Collections.Generic;
using System.Globalization;
using NetworkOperation;
using NetworkOperation.Client;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NetworkOperation.Factories;
using NetworkOperation.Logger;
using Tcp.Core;

namespace Tcp.Client
{
    public class TcpNetOperationClient<TRequest,TResponse> : AbstractClient<TRequest,TResponse,Socket> where TRequest : IOperationMessage, new() where TResponse : IOperationMessage, new()
    {
        public Socket Client { get; }
        private Task _pollTask;
        private bool _prevConnectState;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        
        public override void Connect(string address, int port)
        {
            ConnectAsync(address,port).RunSynchronously();
        }

        void PollEvents()
        {
            _pollTask = Task.Factory.StartNew(async () =>
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    var connected = Client.IsConnected();
                    await Dispatch();
                    if (!connected && _prevConnectState)
                    {
                        CloseSession();
                    }
                    _prevConnectState = connected;
                    await Task.Delay(PollTimeInMs);
                }
                
            }, TaskCreationOptions.LongRunning);
        }

        public override async Task ConnectAsync(string address, int port)
        {
            await Client.ConnectAsync(address, port);
            OpenSession(Client);
            PollEvents();
        }

        public override void Disconnect()
        {
            if (_pollTask == null) return;

            Client.Close();
            _cts.Cancel();
            _pollTask.Wait();
        }

        public override async Task DisconnectAsync()
        {
            if (_pollTask == null) return;
            await Task.Run(async () =>
            {
                Client.Close();
                while (!Client.Connected)
                {
                   await Task.Delay(10);
                }
            });

            _cts.Cancel();
            await _pollTask;
        }


        public TcpNetOperationClient(IFactory<Socket, Session> sessionFactory, IFactory<Session, IClientOperationExecutor> executorFactory, BaseDispatcher<TRequest, TResponse> dispatcher, IStructuralLogger logger) : base(sessionFactory, executorFactory, dispatcher, logger)
        {
            Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Client.LingerState.Enabled = false;
            Client.Blocking = false;
        }
    }
}