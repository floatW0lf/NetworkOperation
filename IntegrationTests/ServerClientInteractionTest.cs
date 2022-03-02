using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IntegrationTests.Client;
using IntegrationTests.Contract;
using IntegrationTests.Server;
using LiteNet.Infrastructure.Host;
using LiteNetLib;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetLibOperation.LiteNet;
using NetworkOperation.Client;
using NetworkOperation.Core;
using NetworkOperation.Core.Dispatching;
using NetworkOperation.Core.Messages;
using NetworkOperation.Core.Models;
using NetworkOperation.Host;
using NetworkOperation.Infrastructure.Client;
using NetworkOperation.Infrastructure.Client.LiteNet;
using NetworkOperation.Infrastructure.Host;
using Serializer.MessagePack;
using Xunit;

namespace IntegrationTests
{
    public class ServerClientInteractionTest : IDisposable
    {
        private const string Address = "localhost:8888";
        private IHostedService _host;
        private IClient _client;
        private IServiceProvider _clientProvider;
        private IServiceProvider _hostProvider;
        
        public ServerClientInteractionTest()
        {
            var clientCollection = new ServiceCollection();
            clientCollection.AddLogging();
            clientCollection
                .NetworkOperationClient<DefaultMessage, DefaultMessage>()
                .Serializer<MsgSerializer>()
                .Executor()
                .RuntimeModel(OperationRuntimeModel.CreateFromAttribute())
                .Dispatcher<ExpressionDispatcher<DefaultMessage,DefaultMessage>>()
                .RegisterHandler<ClientOpHandler>(Scope.Session)
                .UseLiteNet();

            _clientProvider = clientCollection.BuildServiceProvider(false);
            
            var hostCollection = new ServiceCollection();
            hostCollection.AddLogging();
            hostCollection.NetworkOperationHost<DefaultMessage, DefaultMessage>()
                .Executor()
                .Serializer<MsgSerializer>()
                .ConnectHandler<TestSessionRequestHandler>()
                .Dispatcher<ExpressionDispatcher<DefaultMessage, DefaultMessage>>()
                .RuntimeModel(OperationRuntimeModel.CreateFromAttribute())
                .RegisterHandler<LongTimeOperationHandler>(Scope.Single)
                .RegisterHandler<MultiplayHandler>(Scope.Session)
                .RegisterHandler<PushTestHandler>(Scope.Session)
                .UseLiteNet();

            _hostProvider = hostCollection.BuildServiceProvider(false);
        }
        
        [Fact]
        public async Task multiply_handler()
        {
            await StartServerAndClient();
            var operationResult = await _client.Executor.Execute(new Multiply() {A = 100, B = 100}, t => t);
            operationResult.Match(t => t, (state, f) => {}, (status, f) => {}, (ext, f) => {});
            
            Assert.True(operationResult.Is(MultiplyStatus.OverFlow, t=> t));
            Assert.Equal(10000,operationResult.Result);
        }


        [Fact]
        public async Task cancel_operation()
        {
            await StartServerAndClient();
            var source = new CancellationTokenSource();
            using (source)
            {
                source.CancelAfter(TimeSpan.FromMilliseconds(300));
                await Assert.ThrowsAnyAsync<TaskCanceledException>(() =>
                    _client.Executor.Execute<LongTimeOperation, int>(new LongTimeOperation(), source.Token));
            }
        }

        [Fact]
        public async Task push_to_client()
        {
            await StartServerAndClient();
            var result = await _client.Executor.Execute<PushTest, string>(new PushTest() {Message = "push"});
            Assert.Equal(BuiltInOperationState.Success,result.Status);
            Assert.Equal("push_server_client",result.Result);
        }

        [Fact]
        public async Task request_connect()
        {
            await StartServerAndClient(true);
            
            var rejectedCount = 0;
            var connectedCount = 0;
            var sessionEvents = ((ISessionEvents) _client);
            sessionEvents.SessionClosed += session =>
            {
                if (session.GetReason() == DisconnectReason.ConnectionRejected) rejectedCount++;
            }; 
            sessionEvents.SessionClosed += session => { connectedCount++; };

            await Assert.ThrowsAsync<TaskCanceledException>(()=>_client.ConnectAsync(Address,new ExampleConnectPayload() {Authorize = "token", Version = "1", AppId = "some_app" }));
            await _client.ConnectAsync(Address,new ExampleConnectPayload() {Authorize = "token", Version = "1.1", AppId = "some_app" });
            
            Assert.Equal(1,connectedCount);
            Assert.Equal(1,rejectedCount);
        }

        [Fact]
        public async Task disconect_payload()
        {
            await StartServerAndClient();
            var sessionEvents = (ISessionEvents) _client;
            var closed = 0;
            byte[] payload = null;
            sessionEvents.SessionClosed += session =>
            {
                if (session.GetReason() == DisconnectReason.RemoteConnectionClose) closed++;
                payload = session.GetPayload();
            };
            var events = (IHostContext) _host;
            Assert.Equal(SessionState.Opened, events.Sessions.First().State);
            events.Sessions.First().Close(new byte[]{1,1,1});
            await Task.Delay(100);
            Assert.Equal(1,closed);
            Assert.Equal(new byte[]{1,1,1},payload);
        }
        
        private async Task StartServerAndClient(bool onlyHost = false)
        {
            _host = _hostProvider.GetRequiredService<IHostedService>();
            _client = _clientProvider.GetRequiredService<IClient>();
            
            await _host.StartAsync(default);
            if (!onlyHost)
            {
                await _client.ConnectAsync(Address);
            }
        }


        public void Dispose()
        {
            Task.WaitAll(_client.DisconnectAsync(), _host.StopAsync(default));
            _client.Dispose();
        }
    }
}