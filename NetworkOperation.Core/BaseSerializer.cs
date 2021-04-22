using System;
using System.Threading.Tasks;
using NetworkOperation.Core.Dispatching;

namespace NetworkOperation.Core
{
    public abstract class BaseSerializer
    {
        public abstract TypeMessage ReadMessageType(ArraySegment<byte> rawBytes);
        public abstract T Deserialize<T>(ArraySegment<byte> rawBytes, Session context);
        public abstract byte[] Serialize<T>(T obj, Session context);
        public abstract Task<T> DeserializeAsync<T>(ArraySegment<byte> rawBytes, Session context);
        public abstract Task<byte[]> SerializeAsync<T>(T obj, Session context);
    }
}