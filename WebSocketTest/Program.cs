using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NetworkOperation.Core;
using NetworkOperation.Core.Dispatching;
using NetworkOperation.Core.Messages;
using NetworkOperation.Core.Models;
using NetworkOperation.Host;
using NetworkOperation.Infrastructure.Host;
using NetworkOperation.Infrastructure.Host.WebSockets;
using Serializer.MessagePack;
using WebGL.WebSockets.Tests;

namespace WebSocketTest
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            TaskScheduler.UnobservedTaskException += (sender, eventArgs) =>
            {
                eventArgs.SetObserved();
                Console.WriteLine($"UnobservedTaskException {eventArgs.Exception?.Message}");
            };
            var hostCollection = new ServiceCollection();
            
            hostCollection.AddLogging(b => b.AddConsole());
            
            hostCollection.NetworkOperationHost<DefaultMessage, DefaultMessage>()
                .Executor()
                .Serializer<MsgSerializer>()
                .RegisterStatusCodes()
                .ConnectHandler<WebSocketRequestHandler>()
                .Dispatcher<ExpressionDispatcher<DefaultMessage, DefaultMessage>>()
                .RuntimeModel(OperationRuntimeModel.CreateFromAttribute())
                .RegisterHandlers(new [] {typeof(Program).Assembly})
                .UseHttpListenerWebSockets(h =>
                {
                    h.UriHost = "http://*:7700/connection/";
                });

            var host = hostCollection.BuildServiceProvider(false);
            var hostContext = host.GetService<IHostContext>();
            var service = (IHostedService) hostContext;
            await service.StartAsync(default);
            var prevPlayerCount = -1;

            var bigBuffer = Enumerable.Repeat<byte>(125, 64000).ToArray();
            while (true)
            {
               // await hostContext.Executor.Execute(new ClientOp() {Message = "hello"}, t => t);
                //await hostContext.Executor.Execute(new LargeDataOperation() {Raw = bigBuffer}, t => t);
                await Task.Delay(1000);
                if (prevPlayerCount != hostContext.Sessions.Count)
                {
                    Console.WriteLine($"Users: {hostContext.Sessions.Count}");
                    prevPlayerCount = hostContext.Sessions.Count;
                }
            }
        }
    }
}