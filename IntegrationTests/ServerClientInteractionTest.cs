using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IntegrationTests.Client;
using IntegrationTests.Contract;
using IntegrationTests.Server;
using LiteNetLib;
using Microsoft.Extensions.Hosting;
using NetLibOperation;
using NetworkOperation;
using NetworkOperation.Client;
using NetworkOperation.Host;
using NetworkOperation.Server;
using Ninject;
using Xunit;

namespace IntegrationTests
{
    public class ServerClientInteractionTest : IDisposable
    {
        private const string Address = "localhost:8888";
        private IHostedService _host;
        private IClient _client;
        private IKernel _kernel;
        
        public ServerClientInteractionTest()
        {
            _kernel = new StandardKernel(new TestModule(false));
        }
        
        [Fact]
        public async Task multiply_handler()
        {
            await StartServerAndClient();
            var operationResult = await _client.Executor.Execute<Multiply, float>(new Multiply() {A = 100, B = 100});
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
            _kernel.Unbind<SessionRequestHandler>();
            _kernel.Bind<SessionRequestHandler>().To<TestSessionRequestHandler>().InSingletonScope();
            
            _host = _kernel.Get<IHostedService>();
            _client = _kernel.Get<IClient>();
            
            await _host.StartAsync(default);

            var rejectedCount = 0;
            var connectedCount = 0;
            var sessionEvents = ((ISessionEvents) _client);
            sessionEvents.SessionClosed  += session =>
            {
                if (session.GetReason() == DisconnectReason.ConnectionRejected) rejectedCount++;
            }; 
            sessionEvents.SessionClosed += session => { connectedCount++; }; 
            
            _client.ConnectionPayload = new PayloadResolver<ExampleConnectPayload>(
                new ExampleConnectPayload() {Authorize = "token", Version = "1", AppId = "some_app"},
                _kernel.Get<BaseSerializer>());
            await Assert.ThrowsAsync<TaskCanceledException>(()=>_client.ConnectAsync(Address));
            
            _client.ConnectionPayload = new PayloadResolver<ExampleConnectPayload>(
                new ExampleConnectPayload() {Authorize = "token", Version = "1.1", AppId = "some_app"},
                _kernel.Get<BaseSerializer>());
            await _client.ConnectAsync(Address);
            
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
        
        private async Task StartServerAndClient()
        {
            _host = _kernel.Get<IHostedService>();
            _client = _kernel.Get<IClient>();
            await _host.StartAsync(default);
            await _client.ConnectAsync(Address);
        }


        public void Dispose()
        {
            Task.WaitAll(_client.DisconnectAsync(), _host.StopAsync(default));
            _client.Dispose();
        }
    }
}