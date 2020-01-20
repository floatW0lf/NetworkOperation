using System;
using LiteNet.Infrastructure.Client;
using Microsoft.Extensions.DependencyInjection;
using NetworkOperation;
using NetworkOperation.Client;
using NetworkOperation.Dispatching;
using NetworkOperation.Infrastructure.Client;
using Serializer.MessagePack;
using Xunit;

namespace NetOperationTest
{
    public class InfrastructureTests
    {
        [Fact]
        public void must_configure()
        {
            var collection = new ServiceCollection();
            collection
                .NetworkOperationClient<DefaultMessage, DefaultMessage>()
                .ConsoleLogger()
                .ConfigureSerializer<MsgSerializer>()
                .ConfigureExecutor()
                .ConfigureRuntimeModel(OperationRuntimeModel.CreateFromAttribute(new []{typeof(Op)}))
                .ConfigureDispatcher<ExpressionDispatcher<DefaultMessage,DefaultMessage>>()
                .ConfigureLiteNet();

            var p = collection.BuildServiceProvider();
            p.GetRequiredService<IClient>();

        }
    }
}