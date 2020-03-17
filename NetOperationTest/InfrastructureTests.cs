using LiteNet.Infrastructure.Client;
using LiteNet.Infrastructure.Host;
using Microsoft.Extensions.DependencyInjection;
using NetLibOperation;
using NetLibOperation.Client;
using NetworkOperation.Client;
using NetworkOperation.Core.Dispatching;
using NetworkOperation.Core.Messages;
using NetworkOperation.Core.Models;
using NetworkOperation.Host;
using NetworkOperation.Infrastructure.Client;
using NetworkOperation.Infrastructure.Host;
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
            collection.AddLogging();
            collection
                .NetworkOperationClient<DefaultMessage, DefaultMessage>()
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
            collection.AddLogging();
            
            collection.NetworkOperationHost<DefaultMessage, DefaultMessage>()
                .Executor()
                .Serializer<MsgSerializer>()
                .ConnectHandler<DefaultLiteSessionOpenHandler>()
                .Dispatcher<ExpressionDispatcher<DefaultMessage, DefaultMessage>>()
                .RuntimeModel(OperationRuntimeModel.CreateFromAttribute(new[] {typeof(Op)}))
                .UseLiteNet();

            var p = collection.BuildServiceProvider();
            Assert.IsType<NetLibHost<DefaultMessage,DefaultMessage>>(p.GetRequiredService<IHostContext>());


        }
    }
}