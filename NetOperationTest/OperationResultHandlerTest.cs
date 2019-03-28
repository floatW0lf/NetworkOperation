using System;
using Moq;
using NetworkOperation;
using NetworkOperation.OperationResultHandler;
using NetworkOperation.StatusCodes;
using Xunit;

namespace NetOperationTest
{
    public class OperationResultHandlerTest : IDisposable
    {
        [Fact]
        public void OperationResultTest()
        {
            var result = new OperationResult<int>(100, 1);
            var successes = false; 
            var error = false;
            result.Handle().Success(i => { successes = true;}).BuiltInCode(BuiltInOperationState.InternalError,
                () => { error = true; });
            
            Assert.False(successes);
            Assert.True(error);
        }
        
        private enum CustomCodes : uint
        {
            WrongToken = 51,
            Kicked = 52
        }
        [Fact]
        public void OperationResultCustomCodeTest()
        {
            StatusEncoding.Register(typeof(CustomCodes));
            var delMoq = new Mock<Action>();
            var successMoq = new Mock<Action<int>>();
            
            var result = new OperationResult<int>(100, 52);
            
            result.Handle()
                .Success(successMoq.Object)
                .BuiltInCode(BuiltInOperationState.InternalError, delMoq.Object)
                .CustomCode(CustomCodes.Kicked, delMoq.Object);
            
            delMoq.Verify(action => action(),Times.Once);
            successMoq.Verify(action => action(100), Times.Never);
        }

        public void Dispose()
        {
            StatusEncoding.UnregisterAll();
        }
    }
}