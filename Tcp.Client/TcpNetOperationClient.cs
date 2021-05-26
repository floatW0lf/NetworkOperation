using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetworkOperation.Client;
using NetworkOperation.Core;
using NetworkOperation.Core.Dispatching;
using NetworkOperation.Core.Factories;
using NetworkOperation.Core.Messages;
using Tcp.Core;

namespace Tcp.Client
{
    public class TcpNetOperationClient<TRequest,TResponse> : AbstractClient<TRequest,TResponse,Socket> where TRequest : IOperationMessage, new() where TResponse : IOperationMessage, new()
    {
        public Socket Client { get; }
        private Task _pollTask;
        private bool _prevConnectState;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        
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
        public override async Task ConnectAsync<T>(EndPoint remote, T payload, CancellationToken cancellationToken = default)
        {
            await Client.ConnectAsync(remote);
            OpenSession(Client);
            PollEvents();
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

        public override void Dispose()
        {
        }
        public TcpNetOperationClient(IFactory<Socket, Session> sessionFactory, IFactory<Session, IClientOperationExecutor> executorFactory, BaseDispatcher<TRequest, TResponse> dispatcher, ILoggerFactory logger, BaseSerializer serializer) : base(sessionFactory, executorFactory, dispatcher, serializer,logger)
        {
            Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Client.LingerState.Enabled = false;
            Client.Blocking = false;
        }
    }
}