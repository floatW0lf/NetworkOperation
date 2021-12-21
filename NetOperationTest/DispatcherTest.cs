using System;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NetworkOperation.Core;
using NetworkOperation.Core.Dispatching;
using NetworkOperation.Core.Messages;
using NetworkOperation.Core.Models;
using Serializer.MessagePack;
using Xunit;

namespace NetOperationTest
{
    public class DispatcherTest
    {
        
        [DataContract]
        [Operation(0,Handle = Side.All)]
        public struct A : IOperation<int>
        {
            [DataMember]
            public int Arg;

        }
        [DataContract]
        [Operation(1, Handle = Side.Client)]
        public struct B : IOperation<float>
        {
            [DataMember]
            public int Arg;
        }
        
        class TestDispatcher : BaseDispatcher<DefaultMessage,DefaultMessage>
        {
            public TestDispatcher(BaseSerializer serializer, IHandlerFactory factory, OperationRuntimeModel model, ILoggerFactory logger, DescriptionRuntimeModel descriptionRuntimeModel) : base(serializer, factory, model, logger, descriptionRuntimeModel)
            {
            }

            protected override Task<DataWithStateCode> ProcessHandler(DefaultMessage header, RequestContext<DefaultMessage> context, CancellationToken token)
            {
                switch (header.OperationCode)
                {
                    case 0: return GenericHandle<A,int>(header, context, token);
                    case 1: return GenericHandle<B,float>(header, context, token);
                }
                throw new InvalidOperationException();
            }
        }
        
        [Fact]
        public async Task ManualDispatchTest()
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true }).Customize(new SupportMutableValueTypesCustomization());
            var op = fixture.Create<A>();
            var moqHandler = fixture.Freeze<Mock<IHandler<A, int, DefaultMessage>>>();
            var result = fixture.Freeze<int>();
            var factory = new Mock<IHandlerFactory>();
            factory.Setup(f => f.Create<A, int,DefaultMessage>(It.IsAny<RequestContext<DefaultMessage>>())).ReturnsUsingFixture(fixture);
            var model = new OperationRuntimeModel(new[]
            {
                new OperationDescription(0, typeof(A), typeof(int), Side.Server,DeliveryMode.Ordered, DeliveryMode.Ordered, true),
                new OperationDescription(1, typeof(B), typeof(float), Side.Server, DeliveryMode.Ordered, DeliveryMode.Ordered , true)
            });
            var dispatcher = new TestDispatcher(new MsgSerializer(), factory.Object, model, new NullLoggerFactory(),new DescriptionRuntimeModel());
            dispatcher.Subscribe(new Mock<IResponseReceiver<DefaultMessage>>().Object);
            var hasData = true;
            var sessionMock = new Mock<Session>(Array.Empty<SessionProperty>());
            sessionMock.SetupGet(s => s.HasAvailableData).Returns(() => hasData);
            sessionMock.Setup(s => s.ReceiveMessageAsync()).ReturnsAsync(() =>
            {
                hasData = false;
                return CreateRawMessage(0, op, TypeMessage.Request); 
            });
            await dispatcher.DispatchAsync(sessionMock.Object);
            sessionMock.Verify(s => s.ReceiveMessageAsync(), Times.Once);
            moqHandler.Verify(handler => handler.Handle(op, It.IsAny<RequestContext<DefaultMessage>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GeneratedDispatchTest()
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true }).Customize(new SupportMutableValueTypesCustomization());
            var opA = fixture.Create<A>();
            var opB = fixture.Create<B>();
            var resultA = fixture.Freeze<int>();
            var handlerA = fixture.Freeze<Mock<IHandler<A, int,DefaultMessage>>>();
            var handlerB = fixture.Freeze<Mock<IHandler<B, float,DefaultMessage>>>();
            var factory = new Mock<IHandlerFactory>();
            factory.Setup(f => f.Create<A, int,DefaultMessage>(It.IsAny<RequestContext<DefaultMessage>>())).ReturnsUsingFixture(fixture);
            factory.Setup(f => f.Create<B, float,DefaultMessage>(It.IsAny<RequestContext<DefaultMessage>>())).ReturnsUsingFixture(fixture);
            
            var generatedDispatcher = new ExpressionDispatcher<DefaultMessage,DefaultMessage>(new MsgSerializer(), factory.Object, OperationRuntimeModel.CreateFromAttribute(new[] { typeof(A), typeof(B) }), new NullLoggerFactory(), new DescriptionRuntimeModel());
            generatedDispatcher.ExecutionSide = Side.Server;
            
            generatedDispatcher.Subscribe(new Mock<IResponseReceiver<DefaultMessage>>().Object);
            var mockSession = new Mock<Session>(Array.Empty<SessionProperty>());
            var hasData = true;
            mockSession.SetupGet(s => s.HasAvailableData).Returns(() => hasData);
            mockSession.Setup(s => s.ReceiveMessageAsync()).ReturnsAsync(() =>
            {
                hasData = false;
                return CreateRawMessage(0, opA, TypeMessage.Request);
            });

            await generatedDispatcher.DispatchAsync(mockSession.Object);            
            mockSession.Verify(s => s.SendMessageAsync(It.IsAny<ReadOnlyMemory<byte>>(),DeliveryMode.Reliable | DeliveryMode.Ordered), Times.Once);
            handlerA.Verify(handler => handler.Handle(opA,It.IsAny<RequestContext<DefaultMessage>>(), It.IsAny<CancellationToken>()), Times.Once);
            handlerB.Verify(h => h.Handle(It.IsAny<B>(), It.IsAny<RequestContext<DefaultMessage>>(),It.IsAny<CancellationToken>()), Times.Never);
        }

        private static byte[] CreateRawMessage<T>(uint code, T op, TypeMessage type)
        {
            var opt = MessagePackSerializerOptions.Standard.WithResolver(CompositeResolver.Create(new [] {new StatusCodeFormatter()}, new []{StandardResolver.Instance}));
            var subOp = MessagePackSerializer.Serialize(op,opt);
            return MessagePackSerializer.Serialize(new DefaultMessage() {OperationCode = code, OperationData = subOp, Type = type},opt);
        }
    }
}