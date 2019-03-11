using System;

namespace Tcp.Core
{
    public static class ArraySegmentExtensions
    {
        public static ArraySegment<T> Reset<T>(this ArraySegment<T> segment)
        {
            return new ArraySegment<T>(segment.Array);
        }

        public static ArraySegment<T> NewSegment<T>(this ArraySegment<T> segment, int offset, int count)
        {
            return new ArraySegment<T>(segment.Array, offset, count);
        }
    }
}