using System;
using System.IO;
using System.Threading.Tasks;
using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using NetworkOperation.Core;
using NetworkOperation.Core.Dispatching;

namespace WebGL.WebSockets.Tests
{
    public class MsgSerializer : BaseSerializer
    {

        public override TypeMessage ReadMessageType(ArraySegment<byte> rawBytes)
        {
            return MessagePackSerializer.Deserialize<TypeMessage>(rawBytes.Slice(1, 1));
        }

        public override T Deserialize<T>(ArraySegment<byte> rawBytes, Session context)
        {
            return MessagePackSerializer.Deserialize<T>(rawBytes);
        }

        public override byte[] Serialize<T>(T obj,Session context)
        {
            return MessagePackSerializer.Serialize(obj);
        }

        public override async Task<T> DeserializeAsync<T>(ArraySegment<byte> rawBytes, Session context)
        {
            using (var memory = new MemoryStream(rawBytes.ToArray()))
            {
                return await MessagePackSerializer.DeserializeAsync<T>(memory);
            }
        }

        public override async Task<byte[]> SerializeAsync<T>(T obj, Session context)
        {
            using (var memory = new MemoryStream())
            {
                await MessagePackSerializer.SerializeAsync(memory, obj);
                return memory.ToArray();
            }
        }
    }
    public class StatusCodeFormatter : IMessagePackFormatter<StatusCode>
    {
        public void Serialize(ref MessagePackWriter writer, StatusCode value, MessagePackSerializerOptions options)
        {
            writer.WriteUInt32(value.Code);
        }

        public StatusCode Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            return (StatusCode) reader.ReadUInt32();
        }
    }
}