using System;
using System.Buffers;

namespace NetworkOperation.WebSockets.Host
{
    public static class PooledArraySegment
    {
        public static ArraySegment<T> Rent<T>(int size)
        {
            return new ArraySegment<T>(ArrayPool<T>.Shared.Rent(size), 0, size);
        }
        public static void Return<T>(ArraySegment<T> segment)
        {
            ArrayPool<T>.Shared.Return(segment.Array);
        }

        public static void Advance<T>(ref ArraySegment<T> segment, int size)
        {
            var fullSize = segment.Offset + segment.Count;
            var newSize = fullSize + size;
            //fast resize if enough capacity
            if (newSize <= segment.Array.Length)
            {
                segment = new ArraySegment<T>(segment.Array, segment.Offset, segment.Count + size);
                return;
            }
            var advanceMemory = ArrayPool<T>.Shared.Rent(newSize);
            Buffer.BlockCopy(segment.Array, 0, advanceMemory,0, fullSize);
            ArrayPool<T>.Shared.Return(segment.Array);
            segment = new ArraySegment<T>(advanceMemory, segment.Offset, segment.Count + size);

        }
        
    }
}