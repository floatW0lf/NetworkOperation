using System;
using NetworkOperation;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetworkOperation.Factories;
using NetworkOperation.Host;

namespace Tcp.Server
{
    public class TcpNetOperationHost<TRequest,TResponse> : AbstractHost<TRequest,TResponse, Socket> where TRequest : IOperationMessage, new() where TResponse : IOperationMessage, new()
    {
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
                    var newSocket = await Listener.AcceptAsync();
                    
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

        private void Shutdown()
        {
            ShutdownAsync().RunSynchronously();
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

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public TcpNetOperationHost(IFactory<Socket, MutableSessionCollection> sessionsFactory, IFactory<SessionCollection, IHostOperationExecutor> executorFactory, BaseDispatcher<TRequest, TResponse> dispatcher, SessionRequestHandler handler, ILoggerFactory loggerFactory) : base(sessionsFactory, executorFactory, dispatcher, handler, loggerFactory)
        {
        }
    }
}
