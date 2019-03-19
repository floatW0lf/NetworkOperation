using System;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using MessagePack;
using Moq;
using NetworkOperation;
using NetworkOperation.Dispatching;
using NetworkOperation.Extensions;
using NetworkOperation.Server;
using Xunit;
using Xunit.Sdk;

namespace NetOperationTest
{
    public class CancelOperationTest
    {
        public class FooTestHandler : IHandler<Foo,int,DefaultMessage>
        {
            public Task<OperationResult<int>> Handle(Foo objectData, RequestContext<DefaultMessage> context, CancellationToken token)
            {
                return Task.Run(() =>
                {
                    for (int i = 0; i < 10; i++)
                    {
                        Thread.Sleep(20);
                        token.ThrowIfCancellationRequested();
                    }
                    
                    return this.Return(10);
                }, token);
            }
        }
        
        [Operation(0)]
        [DataContract]
        public struct Foo : IOperation<Foo,int>
        {
            [DataMember]
            public int _1;
            [DataMember]
            public float _2;
        }

        [Fact]
        public void ExecutorCancelTest()
        {
            var serializeMock = new Mock<BaseSerializer>();
            var mockSession = new Mock<SessionCollection>();
            var mockSetting = new ServerOperationExecutor<DefaultMessage,DefaultMessage>(OperationRuntimeModel.CreateFromAttribute(new [] { typeof(Foo) }),serializeMock.Object, mockSession.Object);
            var cts = new CancellationTokenSource();
            var task = mockSetting.Execute<Foo,int>(new Foo(), cts.Token);
            cts.Cancel();
            Assert.Equal(TaskStatus.Canceled,task.Status);
            mockSession.Verify(c => c.SendToAllAsync(It.IsAny<byte[]>()), Times.Exactly(2));
        }
        
        [Fact]
        public async Task DispatcherCancelTest()
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true }).Customize(new SupportMutableValueTypesCustomization());
            var opFoo = fixture.Create<Foo>();
            
            var fooHandler = new FooTestHandler();
                
            var factory = new Mock<IHandlerFactory>();
            factory.Setup(f => f.Create<Foo, int,DefaultMessage>()).Returns(fooHandler);
            
            var generatedDispatcher = new ExpressionDispatcher<DefaultMessage,DefaultMessage>(new MsgSerializer(), factory.Object, OperationRuntimeModel.CreateFromAttribute(new[] { typeof(Foo) }));
            generatedDispatcher.ExecutionSide = Side.Server;
            
            generatedDispatcher.Subscribe(new Mock<IResponseReceiver<DefaultMessage>>().Object);
            var mockSession = new Mock<Session>();
            var hasData = true;
            
            mockSession.SetupGet(s => s.HasAvailableData).Returns(() => hasData);
            mockSession.Setup(s => s.ReceiveMessageAsync()).ReturnsAsync(() =>
            {
                hasData = false;
                return CreateRawMessage(0, opFoo);
            });
            var hasCancelData = true;
            var mockSessionWithCancel = new Mock<Session>();
            mockSessionWithCancel.SetupGet(s => s.HasAvailableData).Returns(() => hasCancelData);
            mockSessionWithCancel.Setup(s => s.ReceiveMessageAsync()).ReturnsAsync(() =>
            {
                hasCancelData = false;
                return MessagePackSerializer.Serialize(new DefaultMessage()
                    {OperationCode = 0, StateCode = (uint)BuiltInOperationState.Cancel});
            });
            
            generatedDispatcher.DispatchAsync(mockSession.Object).GetAwaiter();
            Task.Delay(100).ContinueWith(task =>
            {
                generatedDispatcher.DispatchAsync(mockSessionWithCancel.Object).GetAwaiter();
            }).GetAwaiter();

            
            await Task.Delay(300);
            mockSession.Verify(s => s.SendMessageAsync(It.IsAny<ArraySegment<byte>>()), Times.Never);
            mockSessionWithCancel.Verify(s => s.SendMessageAsync(It.IsAny<ArraySegment<byte>>()), Times.Never);
            mockSession.Verify(s => s.ReceiveMessageAsync(),Times.Once);
            mockSessionWithCancel.Verify(s => s.ReceiveMessageAsync(),Times.Once);
        }
        private static byte[] CreateRawMessage<T>(uint code, T op)
        {
            var subOp = MessagePackSerializer.Serialize(op);
            return MessagePackSerializer.Serialize(new DefaultMessage() {OperationCode = code, OperationData = subOp });
        }
    }
}