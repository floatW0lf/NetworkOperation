using System;
using System.Threading.Tasks;
using NetworkOperation.Core.Dispatching;

namespace NetworkOperation.Core
{
    public abstract class BaseSerializer
    {
        public abstract TypeMessage ReadMessageType(ReadOnlyMemory<byte> rawBytes);
        public abstract T Deserialize<T>(ReadOnlyMemory<byte> rawBytes, Session context);
        public abstract ReadOnlyMemory<byte> Serialize<T>(T obj, Session context);
        public abstract Task<T> DeserializeAsync<T>(ReadOnlyMemory<byte> rawBytes, Session context);
        public abstract Task<ReadOnlyMemory<byte>> SerializeAsync<T>(T obj, Session context);
    }
}