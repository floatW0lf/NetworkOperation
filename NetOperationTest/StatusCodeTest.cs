using System;
using System.Runtime.CompilerServices;
using NetworkOperation;
using Xunit;

namespace NetOperationTest
{
    public enum FooCodes
    {
        A,
        B,
        C
    }
    
    public enum BarCodes
    {
        A,
        B,
        C
    }
    
    public class StatusCodeTest
    {
        public StatusCodeTest()
        {
            StatusCode.UnregisterAll();
            StatusCode.Register(typeof(FooCodes), typeof(BarCodes));
        }
        
        [Fact]
        public void should_be_converted_from_enum()
        {
            StatusCode b = FooCodes.A;
            Assert.Equal(0000_0001u,b.Code);
            
            StatusCode a = BarCodes.A;
            Assert.Equal(0000_0002u,a.Code);
        }
        
        
        [Fact]
        public void check_operators()
        {
            StatusCode a = BarCodes.A;
            StatusCode b = FooCodes.A;
            StatusCode c = BarCodes.A;

            Assert.Equal(BarCodes.A,a.AsEnum<BarCodes>());
            Assert.NotEqual(a,b);
            Assert.Equal(a,c);
            
            Assert.True(b == FooCodes.A,"b == FooCodes.A");
            Assert.True(b >= FooCodes.A,"b >= FooCodes.A");
            Assert.True(b <= BarCodes.A,"b <= BarCodes.A");
            Assert.True(b < BarCodes.A,"b < BarCodes.A");
            Assert.True(a.Equals(BarCodes.A));
        }
        
        [Fact]
        public void should_be_throw()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                StatusCode a = BarCodes.A;
                StatusCode.Register(typeof(FooCodes), typeof(BarCodes));
            });
        }
        
        
    }
}