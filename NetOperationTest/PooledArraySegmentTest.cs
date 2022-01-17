using System.Linq;
using NetworkOperation.WebSockets.Host;
using Xunit;

namespace NetOperationTest
{
    public class PooledArraySegmentTest
    {
        [Theory]
        [InlineData(100, 100, 28, 28)]
        [InlineData(300, 200, 200, 300)]
        [InlineData(100, 100, 29, 29)]
        [InlineData(100, 100, 27, 27)]
        [InlineData(100, 0, 28, 128)]
        public void Resize(int start, int write, int advance, int expected)
        {
            var segment = PooledArraySegment.Rent<byte>(start);
            var writen = segment.Slice(write);
            PooledArraySegment.Advance(ref writen, advance);
            Assert.Equal(expected, writen.Count);
            PooledArraySegment.Return(segment);
        }
        
        [Fact]
        public void must_copied_valid()
        {
            var source = Enumerable.Range(0, 16).Select(v => (byte)v).ToArray();
            var expected = source.Concat(new byte[] {0, 0, 0, 0}).ToArray();
            var segment = PooledArraySegment.Rent<byte>(source.Length);
            source.CopyTo(segment.Array, 0);
            PooledArraySegment.Advance(ref segment,4);
            Assert.Equal(expected, segment);
        }
    }
}