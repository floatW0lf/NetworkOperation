using System;
using NetworkOperation;
using NetworkOperation.Server;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NetworkOperation.Factories;
using Tcp.Core;

namespace Tcp.Server
{
    public class TcpNetOperationServer<TRequest,TResponse> : AbstractServer<TRequest,TResponse, Socket, Socket> where TRequest : IOperationMessage, new() where TResponse : IOperationMessage, new()
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

                    for (int i = 0; i < Sessions.Count; i++)
                    {
                        Dispatcher.DispatchAsync(Sessions[i]).GetAwaiter();
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
            for (int i = 0; i < Sessions.Count; i++)
            {
                var session = Sessions[i];
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

        public TcpNetOperationServer(IFactory<Socket, MutableSessionCollection> sessionsFactory, IFactory<Socket, Session> sessionFactory, IFactory<SessionCollection, IServerOperationExecutor> executorFactory, BaseDispatcher<TRequest,TResponse> dispatcher) : base(sessionsFactory, sessionFactory, executorFactory, dispatcher)
        {
        }
    }
}
