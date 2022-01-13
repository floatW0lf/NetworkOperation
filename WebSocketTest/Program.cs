using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
            var hostCollection = new ServiceCollection();
            hostCollection.AddLogging();
            hostCollection.NetworkOperationHost<DefaultMessage, DefaultMessage>()
                .Executor()
                .Serializer<MsgSerializer>()
                .ConnectHandler<WebSocketRequestHandler>()
                .Dispatcher<ExpressionDispatcher<DefaultMessage, DefaultMessage>>()
                .RuntimeModel(OperationRuntimeModel.CreateFromAttribute())
                .RegisterHandlers(new [] {typeof(Program).Assembly})
                .UseWebSockets(h =>
                {
                    h.UriHost = "http://localhost:7777/";
                });

            var host = hostCollection.BuildServiceProvider(false);
            var hostContext = host.GetService<IHostContext>();
            var service = (IHostedService) hostContext;
            await service.StartAsync(default);
            while (true)
            {
                await hostContext.Executor.Execute(new ClientOp(){Message = "hello"}, t => t);
                await Task.Delay(1000);
            }
        }
    }
}