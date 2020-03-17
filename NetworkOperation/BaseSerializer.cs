using System;
using System.Threading.Tasks;

namespace NetworkOperation.Core
{
    public abstract class BaseSerializer
    {
        public abstract T Deserialize<T>(ArraySegment<byte> rawBytes);
        public abstract byte[] Serialize<T>(T obj);
        public abstract Task<T> DeserializeAsync<T>(ArraySegment<byte> rawBytes);
        public abstract Task<byte[]> SerializeAsync<T>(T obj);
    }
}