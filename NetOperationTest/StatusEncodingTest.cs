using System;
using System.Text;
using NetworkOperation;
using NetworkOperation.StatusCodes;
using Xunit;

namespace NetOperationTest
{
    public enum TestValidStatusCode : uint
    {
        SomeCode = 51,
        SomeCode2 = 52,
        SomeCode3 = 53
    }
    public enum TestValidOtherStatusCode : uint
    {
        SomeCode = 54,
        SomeCode2 = 55,
        SomeCode3 = 56
    }
    
    public enum TestWrongStatusCode : uint
    {
        SomeCode = 56,
        SomeCode2 = 57,
        SomeCode3 = 58
    }

    public class StatusEncodingTest : IDisposable
    {
        [Fact]
        public void EnumRegisterTest()
        {
            StatusEncoding.Register(typeof(TestValidStatusCode), typeof(TestValidOtherStatusCode));
            Assert.True(StatusEncoding.IsValidValue<TestValidStatusCode>(52));
            Assert.True(StatusEncoding.IsValidValue<TestValidOtherStatusCode>(56));
            
        }
        
        [Fact]
        public void WrongEnumRegisterTest()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                StatusEncoding.Register(typeof(TestValidStatusCode), typeof(TestValidOtherStatusCode),
                    typeof(TestWrongStatusCode));
            });

        }

        [Fact]
        public void Encode_DecodeStatusCode()
        {
            
            StatusEncoding.Register(typeof(TestValidStatusCode), typeof(TestValidOtherStatusCode));
            var message = new DefaultMessage();
            StatusEncoding.Encode(ref message, TestValidStatusCode.SomeCode);
            
            Assert.Equal(51U, message.StatusCode);
            Assert.Equal(TestValidStatusCode.SomeCode, StatusEncoding.Decode(message,TestValidStatusCode.SomeCode3));
        }
        
        [Fact]
        public void WrongStatusCode()
        {
            StatusEncoding.Register(typeof(TestValidStatusCode), typeof(TestValidOtherStatusCode));
            var message = new DefaultMessage {StatusCode = 100};
            Assert.Throws<InvalidOperationException>(() =>
            {
                StatusEncoding.Decode(message, TestValidStatusCode.SomeCode);
            });
            
        }

        public void Dispose()
        {
            StatusEncoding.UnregisterAll();
        }
    }
}