using System;
using System.Threading.Tasks;
using IntegrationTests.Client;
using IntegrationTests.Contract;
using IntegrationTests.Server;
using Microsoft.Extensions.Hosting;
using NetworkOperation.Client;
using NetworkOperation.Server;
using Ninject;
using Xunit;

namespace IntegrationTests
{
    public class ServerClientInteractionTest
    {
        
        [Fact]
        public async Task multiply_handler()
        {
            var client = await CreateServerAndClient();
            var operationResult = await client.Executor.Execute<PlusOp, float>(new PlusOp() {A = 100, B = 100});
            Assert.Equal(200,operationResult.Result);
        }

        private static async Task<IClient> CreateServerAndClient()
        {
            var kernel = new StandardKernel(new TestModule(false));
            var host = kernel.Get<IHostedService>();
            await host.StartAsync(default);
            var client = kernel.Get<IClient>();
            await client.ConnectAsync("localhost:8888");
            return client;
        }
    }
}