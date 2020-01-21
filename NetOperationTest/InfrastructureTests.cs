using System;
using LiteNet.Infrastructure.Client;
using LiteNet.Infrastructure.Host;
using Microsoft.Extensions.DependencyInjection;
using NetLibOperation;
using NetLibOperation.Client;
using NetworkOperation;
using NetworkOperation.Client;
using NetworkOperation.Dispatching;
using NetworkOperation.Infrastructure.Client;
using NetworkOperation.Infrastructure.Host;
using NetworkOperation.Server;
using Serializer.MessagePack;
using Xunit;

namespace NetOperationTest
{
    public class InfrastructureTests
    {
        [Fact]
        public void must_client_configure()
        {
            var collection = new ServiceCollection();
            collection
                .NetworkOperationClient<DefaultMessage, DefaultMessage>()
                .ConsoleLogger()
                .Serializer<MsgSerializer>()
                .Executor()
                .RuntimeModel(OperationRuntimeModel.CreateFromAttribute(new []{typeof(Op)}))
                .Dispatcher<ExpressionDispatcher<DefaultMessage,DefaultMessage>>()
                .UseLiteNet();

            var p = collection.BuildServiceProvider();
            Assert.IsType<Client<DefaultMessage,DefaultMessage>>(p.GetRequiredService<IClient>());

        }

        [Fact]
        public void must_host_configure()
        {
            var collection = new ServiceCollection();
            collection.NetworkOperationHost<DefaultMessage, DefaultMessage>()
                .Executor()
                .Serializer<MsgSerializer>()
                .ConnectHandler<DefaultLiteSessionOpenHandler>()
                .Dispatcher<ExpressionDispatcher<DefaultMessage, DefaultMessage>>()
                .RuntimeModel(OperationRuntimeModel.CreateFromAttribute(new[] {typeof(Op)}))
                .ConsoleLogger()
                .UseLiteNet();

            var p = collection.BuildServiceProvider();
            Assert.IsType<NetLibHost<DefaultMessage,DefaultMessage>>(p.GetRequiredService<IHostContext>());


        }
    }
}