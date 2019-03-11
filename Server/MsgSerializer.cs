using System;
using System.IO;
using System.Threading.Tasks;
using MessagePack;
using NetworkOperation;

namespace NetOperationTest
{
    public class MsgSerializer : BaseSerializer
    {
        public override T Deserialize<T>(ArraySegment<byte> rawBytes)
        {
            return MessagePackSerializer.Deserialize<T>(rawBytes);
        }

        public override byte[] Serialize<T>(T obj)
        {
            return MessagePackSerializer.Serialize(obj);
        }

        public override async Task<T> DeserializeAsync<T>(ArraySegment<byte> rawBytes)
        {
            using (var memory = new MemoryStream(rawBytes.ToArray()))
            {
                return await MessagePackSerializer.DeserializeAsync<T>(memory);
            }
        }

        public override async Task<byte[]> SerializeAsync<T>(T obj)
        {
            using (var memory = new MemoryStream())
            {
                await MessagePackSerializer.SerializeAsync(memory, obj);
                return memory.ToArray();
            }
        }
    }
}