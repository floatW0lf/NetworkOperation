using System;
using Moq;
using NetworkOperation;
using NetworkOperation.Server;
using System.Threading.Tasks;
using Moq.Protected;
using Xunit;

namespace NetOperationTest
{
    public class BaseOperationExecutorTest
    {
        [Operation(0)]
        public struct A : IOperation<A,int>
        {
            public int _1;
            public float _2;
        }

        [Fact]
        public void ExecuteTest()
        {
            var serializeMock = new Mock<BaseSerializer>();
            var mockSession = new Mock<SessionCollection>();
            var mockSetting = new HostOperationExecutor<DefaultMessage,DefaultMessage>(OperationRuntimeModel.CreateFromAttribute(new [] { typeof(A) }),serializeMock.Object, mockSession.Object);
            var task = mockSetting.Execute<A,int>(new A());
            Assert.Equal(TaskStatus.WaitingForActivation, task.Status);

            mockSession.Verify(c => c.SendToAllAsync(It.IsAny<byte[]>()), Times.Once);
            serializeMock.Verify(serializer => serializer.Serialize(It.IsAny<A>()), Times.Once);
        }

        [Fact]
        public async Task ReceiveResultTest()
        {
            var serializeMock = new Mock<BaseSerializer>();
            
            serializeMock.Setup(serializer => serializer.Deserialize<int>(It.IsAny<ArraySegment<byte>>())).Returns(111);
            serializeMock.Setup(serializer => serializer.Serialize(It.IsAny<A>())).Returns(new byte[10]);
            var executor = new HostOperationExecutor<DefaultMessage,DefaultMessage>(OperationRuntimeModel.CreateFromAttribute(new[] {typeof(A)}), serializeMock.Object, new Mock<SessionCollection>().Object);
            var mockGenerator = new Mock<IGeneratorId>();
            mockGenerator.Setup(id => id.Generate()).Returns(100);
            executor.MessageIdGenerator = mockGenerator.Object;
            IResponseReceiver<DefaultMessage> e = executor;
            
            Task.Delay(100).ContinueWith(_ => e.Receive(new DefaultMessage() {OperationCode = 0, StatusCode = (uint)BuiltInOperationState.Success, OperationData = new byte[10],Id = 100}))
                .GetAwaiter();
            
            var result =  await executor.Execute<A, int>(new A());
            
            Assert.Equal(111, result.Result);
        }
    }
}
