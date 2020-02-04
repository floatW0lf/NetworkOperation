using LiteNetLib;
using NetLibOperation;
using NetworkOperation;
using Xunit;

namespace NetOperationTest
{
    public class DeliveryModeTest
    {
        [Fact]
        public void ConvertToLiteNetLib()
        {
            var abstractMode = DeliveryMode.Sequenced;
            Assert.Equal(DeliveryMethod.Sequenced,abstractMode.Convert());
            abstractMode = DeliveryMode.Reliable | DeliveryMode.Sequenced | DeliveryMode.Ordered;
            Assert.Equal(DeliveryMethod.ReliableOrdered,abstractMode.Convert());
        }
        
    }

    
}