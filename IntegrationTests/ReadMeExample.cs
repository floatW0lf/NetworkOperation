using System;
using System.Threading;
using System.Threading.Tasks;
using LiteNet.Infrastructure.Host;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetworkOperation.Client;
using NetworkOperation.Core;
using NetworkOperation.Core.Dispatching;
using NetworkOperation.Core.Messages;
using NetworkOperation.Core.Models;
using NetworkOperation.Host;
using NetworkOperation.Infrastructure.Client;
using NetworkOperation.Infrastructure.Client.LiteNet;
using NetworkOperation.Infrastructure.Host;
using NetworkOperation.LiteNet.Host;
using Serializer.MessagePack;

namespace Example
{
    [Operation(1000, Handle = Side.Server)]
    public struct PlusOperation : IOperation<int>
    {
        public int A { get; set; }
        public int B { get; set; }
    }
    [Operation(1001, Handle = Side.Client)]
    public struct SendToClientMessage : IOperation<Empty>
    {
        public string Message { get; set; }
    }
    
    public class ReadMe
    {
        public class SendToClientMessageHandler : IHandler<SendToClientMessage, Empty, DefaultMessage>
        {
            public async Task<OperationResult<Empty>> Handle(SendToClientMessage objectData, RequestContext<DefaultMessage> context, CancellationToken token)
            {
                Console.WriteLine(objectData.Message);
                return this.ReturnEmpty();
            }
        }
        async Task Client()
        {
            var clientCollection = new ServiceCollection();
            
            clientCollection
                .NetworkOperationClient<DefaultMessage, DefaultMessage>()
                .Serializer<MsgSerializer>()
                .Executor()
                .RuntimeModel(OperationRuntimeModel.CreateFromAttribute())
                .Dispatcher<ExpressionDispatcher<DefaultMessage,DefaultMessage>>()
                .RegisterHandlers(AppDomain.CurrentDomain.GetAssemblies())
                .UseLiteNet();

            var client = clientCollection.BuildServiceProvider(false).GetService<IClient>();
            await client.ConnectAsync("localhost:8888");
            var result = await client.Executor.Execute<PlusOperation, int>(new PlusOperation(){A = 2, B = 40});
            result = await client.Executor.Execute(new PlusOperation(){A = 2, B = 40}, t => t); //with auto resolve type result operation
            if (result.Is(BuiltInOperationState.Success))
            {
                Console.WriteLine($"Operation result is {result.Result}");
            }

            await client.DisconnectAsync();
        }

        public class PlusOperationHandler : IHandler<PlusOperation, int, DefaultMessage>
        {
            public async Task<OperationResult<int>> Handle(PlusOperation objectData, RequestContext<DefaultMessage> context, CancellationToken token)
            {
                var result = objectData.A + objectData.B;
                await Task.Delay(1000, token);
                return this.Return(result);
            }
        }

        async Task Server()
        {
            var hostCollection = new ServiceCollection();
            hostCollection.NetworkOperationHost<DefaultMessage, DefaultMessage>()
                .Executor()
                .Serializer<MsgSerializer>()
                .ConnectHandler<DefaultLiteSessionOpenHandler>()
                .Dispatcher<ExpressionDispatcher<DefaultMessage, DefaultMessage>>()
                .RuntimeModel(OperationRuntimeModel.CreateFromAttribute())
                .RegisterHandlers(AppDomain.CurrentDomain.GetAssemblies())
                .UseLiteNet();

            var provider = hostCollection.BuildServiceProvider(false);
            var hostedService = provider.GetService<IHostedService>();
            var context = provider.GetService<IHostContext>();
            
            await hostedService.StartAsync(CancellationToken.None);
            
            context.Sessions.SessionOpened += async session =>
            {
                await context.Executor.Execute<SendToClientMessage, Empty>(new SendToClientMessage() { Message = "Hello world" }, new [] {session});
            };
            
            await hostedService.StopAsync(CancellationToken.None);
            
        }
    }
}