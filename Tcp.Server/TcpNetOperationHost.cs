using System;
using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetworkOperation.Core;
using NetworkOperation.Core.Dispatching;
using NetworkOperation.Core.Factories;
using NetworkOperation.Core.Messages;
using NetworkOperation.Host;
using Tcp.Core;

namespace Tcp.Server
{
    public class TcpNetOperationHost<TRequest,TResponse> : AbstractHost<TRequest,TResponse, Socket> where TRequest : IOperationMessage, new() where TResponse : IOperationMessage, new()
    {
        public int ListenPort { get; set; }
        public Socket Listener { get; private set; }

        private Task pollTask;
        private Task acceptConnectionTask;
        

        private CancellationTokenSource cts = new CancellationTokenSource();

        private void Start(int port)
        {
            CreateServerSocket(port);
            
            
            acceptConnectionTask = Task.Factory.StartNew(async () =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    var newConnection = await Listener.AcceptAsync();
                    ArraySegment<byte> buffer = ArrayPool<byte>.Shared.Rent(2000).To();
                    var count = await newConnection.ReceiveAsync(buffer, SocketFlags.None);
                    BeforeSessionOpen(new TcpSessionRequest(newConnection, buffer.Slice(0, count)));
                    
                    await Task.Delay(PollTimeInMs);
                }

            }, TaskCreationOptions.LongRunning);

            pollTask = Task.Factory.StartNew(async () =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    CheckForDisconnectClient();

                    foreach (var session in Sessions)
                    {
                        Dispatcher.DispatchAsync(session).GetAwaiter();
                    }
                    await Task.Delay(PollTimeInMs);
                }
                
            }, TaskCreationOptions.LongRunning);
        }

        private void CreateServerSocket(int port)
        {
            Listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) {Blocking = false};
            Listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            Listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
            Listener.Bind(new IPEndPoint(IPAddress.Any, port));
            Listener.Listen(100);
        }

        private void CheckForDisconnectClient()
        {
            foreach (var session in Sessions)
            {
                if (session.State == SessionState.Closed)
                {
                    SessionClose(session);
                }
            }
        }
        private async Task ShutdownAsync()
        {
            CloseAllSession();
            cts.Cancel();
            await pollTask;
            await acceptConnectionTask;
            Listener.Close();
            Listener = null;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            Start(ListenPort);
            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return ShutdownAsync();
        }

        public TcpNetOperationHost(IFactory<Socket, MutableSessionCollection> sessionsFactory, IFactory<SessionCollection, IHostOperationExecutor> executorFactory, BaseDispatcher<TRequest, TResponse> dispatcher, SessionRequestHandler handler, ILoggerFactory loggerFactory) : base(sessionsFactory, executorFactory, dispatcher, handler, loggerFactory)
        {
        }
    }
}
