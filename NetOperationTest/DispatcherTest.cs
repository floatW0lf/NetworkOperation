using AutoFixture;
using AutoFixture.AutoMoq;
using MessagePack;
using Moq;
using NetworkOperation;
using NetworkOperation.Dispatching;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using NetworkOperation.Logger;
using Xunit;

namespace NetOperationTest
{
    public class DispatcherTest
    {
        [DataContract]
        [Operation(0,Handle = Side.All)]
        public struct A : IOperation<A,int>
        {
            [DataMember]
            public int Arg;

        }
        [DataContract]
        [Operation(1, Handle = Side.Client)]
        public struct B : IOperation<B,float>
        {
            [DataMember]
            public int Arg;
        }
        
        class TestDispatcher : BaseDispatcher<DefaultMessage,DefaultMessage>
        {
            

            protected override Task<DataWithStateCode> ProcessHandler(Session session, DefaultMessage message, OperationDescription operationDescription, CancellationToken token)
            {
                switch (message.OperationCode)
                {
                    case 0: return GenericHandle<A, int>(session, message, operationDescription, token);
                    case 1: return GenericHandle<B, float>(session, message, operationDescription, token);
                }
                throw new Exception("wrong operation");
            }

            public TestDispatcher(BaseSerializer serializer, IHandlerFactory factory, OperationRuntimeModel model, IStructuralLogger logger) : base(serializer, factory, model, logger)
            {
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
            factory.Setup(f => f.Create<A, int,DefaultMessage>()).ReturnsUsingFixture(fixture);
            var model = new OperationRuntimeModel(new[]
            {
                new OperationDescription(0, typeof(A), typeof(int), Side.Server),
                new OperationDescription(1, typeof(B), typeof(float), Side.Server)
            });
            var dispatcher = new TestDispatcher(new MsgSerializer(), factory.Object, model,new Mock<IStructuralLogger>().Object);
            dispatcher.Subscribe(new Mock<IResponseReceiver<DefaultMessage>>().Object);
            var hasData = true;
            var sessionMock = new Mock<Session>();
            sessionMock.SetupGet(s => s.HasAvailableData).Returns(() => hasData);
            sessionMock.Setup(s => s.ReceiveMessageAsync()).ReturnsAsync(() =>
            {
                hasData = false;
                return CreateRawMessage(0, op); 
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
            factory.Setup(f => f.Create<A, int,DefaultMessage>()).ReturnsUsingFixture(fixture);
            factory.Setup(f => f.Create<B, float,DefaultMessage>()).ReturnsUsingFixture(fixture);
            
            var generatedDispatcher = new ExpressionDispatcher<DefaultMessage,DefaultMessage>(new MsgSerializer(), factory.Object, OperationRuntimeModel.CreateFromAttribute(new[] { typeof(A), typeof(B) }), new Mock<IStructuralLogger>().Object);
            generatedDispatcher.ExecutionSide = Side.Server;
            
            generatedDispatcher.Subscribe(new Mock<IResponseReceiver<DefaultMessage>>().Object);
            var mockSession = new Mock<Session>();
            var hasData = true;
            mockSession.SetupGet(s => s.HasAvailableData).Returns(() => hasData);
            mockSession.Setup(s => s.ReceiveMessageAsync()).ReturnsAsync(() =>
            {
                hasData = false;
                return CreateRawMessage(0, opA);
            });

            await generatedDispatcher.DispatchAsync(mockSession.Object);            
            mockSession.Verify(s => s.SendMessageAsync(It.IsAny<ArraySegment<byte>>()), Times.Once);
            handlerA.Verify(handler => handler.Handle(opA,It.IsAny<RequestContext<DefaultMessage>>(), It.IsAny<CancellationToken>()), Times.Once);
            handlerB.Verify(h => h.Handle(It.IsAny<B>(), It.IsAny<RequestContext<DefaultMessage>>(),It.IsAny<CancellationToken>()), Times.Never);
        }

        private static byte[] CreateRawMessage<T>(uint code, T op)
        {
            var subOp = MessagePackSerializer.Serialize(op);
            return MessagePackSerializer.Serialize(new DefaultMessage() {OperationCode = code, OperationData = subOp });
        }
    }
}