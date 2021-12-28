using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using MessagePack;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NetworkOperation.Core;
using NetworkOperation.Core.Dispatching;
using NetworkOperation.Core.Messages;
using NetworkOperation.Core.Models;
using NetworkOperation.Host;
using Serializer.MessagePack;
using Xunit;

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
        public struct Foo : IOperation<int>
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
            var mockSetting = new HostOperationExecutor<DefaultMessage,DefaultMessage>(OperationRuntimeModel.CreateFromAttribute(new [] { typeof(Foo) }),serializeMock.Object, mockSession.Object,new NullLoggerFactory());
            var cts = new CancellationTokenSource();
            var task = mockSetting.Execute<Foo,int>(new Foo(), cts.Token);
            cts.Cancel();
            Assert.Equal(TaskStatus.Canceled,task.Status);
            mockSession.Verify(c => c.SendToAllAsync(It.IsAny<ArraySegment<byte>>(), DeliveryMode.Reliable | DeliveryMode.Ordered), Times.Exactly(2));
        }
        
        [Fact]
        public async Task DispatcherCancelTest()
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true }).Customize(new SupportMutableValueTypesCustomization());
            var opFoo = fixture.Create<Foo>();
            
            var fooHandler = new FooTestHandler();
                
            var factory = new Mock<IHandlerFactory>();
            factory.Setup(f => f.Create<Foo, int,DefaultMessage>(It.IsAny<RequestContext<DefaultMessage>>())).Returns(fooHandler);
            
            var generatedDispatcher = new ExpressionDispatcher<DefaultMessage,DefaultMessage>(
                new MsgSerializer(), 
                factory.Object, 
                OperationRuntimeModel.CreateFromAttribute(new[] { typeof(Foo) }), 
                new NullLoggerFactory(), new DescriptionRuntimeModel());
            
            generatedDispatcher.ExecutionSide = Side.Server;
            
            generatedDispatcher.Subscribe(new Mock<IResponseReceiver<DefaultMessage>>().Object);
            var mockSession = new Mock<Session>(Array.Empty<SessionProperty>());
           
            mockSession.SetupGet(s => s.Bytes).Returns(() => RequestData(0, opFoo));
            
            var mockSessionWithCancel = new Mock<Session>(Array.Empty<SessionProperty>());

            mockSessionWithCancel.SetupGet(s => s.Bytes).Returns(CancelData);
            
            generatedDispatcher.DispatchAsync(mockSession.Object).GetAwaiter();
            Task.Delay(100).ContinueWith(task =>
            {
                generatedDispatcher.DispatchAsync(mockSessionWithCancel.Object).GetAwaiter();
            }).GetAwaiter();

            
            await Task.Delay(300);
            mockSession.Verify(s => s.SendMessageAsync(It.IsAny<ArraySegment<byte>>(), DeliveryMode.Reliable | DeliveryMode.Ordered), Times.Never);
            mockSessionWithCancel.Verify(s => s.SendMessageAsync(It.IsAny<ArraySegment<byte>>(), DeliveryMode.Reliable | DeliveryMode.Ordered), Times.Never);
            mockSession.VerifyGet(s => s.Bytes,Times.Once);
            mockSessionWithCancel.VerifyGet(s => s.Bytes,Times.Once);
        }
        private static byte[] CreateRawMessage<T>(uint code, T op)
        {
            var subOp = MessagePackSerializer.Serialize(op);
            return MessagePackSerializer.Serialize(new DefaultMessage() {OperationCode = code, OperationData = subOp});
        }
        
        private async IAsyncEnumerable<ArraySegment<byte>> RequestData<T>(uint code, T op)
        {
            yield return CreateRawMessage(code, op);
        }

        private async IAsyncEnumerable<ArraySegment<byte>> CancelData()
        {
            yield return MessagePackSerializer.Serialize(new DefaultMessage() {OperationCode = 0, Status = BuiltInOperationState.Cancel});
        }
    }
}