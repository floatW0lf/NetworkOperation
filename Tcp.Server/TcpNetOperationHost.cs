using System;
using NetworkOperation;
using NetworkOperation.Server;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NetworkOperation.Factories;
using NetworkOperation.Host;
using Tcp.Core;

namespace Tcp.Server
{
    public class TcpNetOperationHost<TRequest,TResponse> : AbstractHost<TRequest,TResponse, Socket, Socket> where TRequest : IOperationMessage, new() where TResponse : IOperationMessage, new()
    {
        public Socket Listener { get; private set; }

        private Task pollTask;
        private Task acceptConnectionTask;
        

        private CancellationTokenSource cts = new CancellationTokenSource();

        public override void Start(int port)
        {
            CreateServerSocket(port);
            ServerStarted(Listener);
            
            acceptConnectionTask = Task.Factory.StartNew(async () =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    var newSocket = await Listener.AcceptAsync();
                    SessionOpen(newSocket);
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
                    session.Close();
                }
            }
        }

        public override void Shutdown()
        {
            ShutdownAsync().RunSynchronously();
        }

        public override async Task ShutdownAsync()
        {
            CloseAllSession();
            cts.Cancel();
            await pollTask;
            await acceptConnectionTask;
            Listener.Close();
            Listener = null;
        }

        public TcpNetOperationHost(IFactory<Socket, MutableSessionCollection> sessionsFactory, IFactory<Socket, Session> sessionFactory, IFactory<SessionCollection, IHostOperationExecutor> executorFactory, BaseDispatcher<TRequest,TResponse> dispatcher) : base(sessionsFactory, sessionFactory, executorFactory, dispatcher)
        {
        }
    }
}
