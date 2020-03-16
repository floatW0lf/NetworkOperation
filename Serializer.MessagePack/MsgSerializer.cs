using System;
using System.IO;
using System.Threading.Tasks;
using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using NetworkOperation;
using System.Linq;

namespace Serializer.MessagePack
{
    public class MsgSerializer : BaseSerializer
    {
        private static MessagePackSerializerOptions _options;
        static MsgSerializer()
        {
            
            _options = MessagePackSerializerOptions.Standard.WithResolver(CompositeResolver.Create(new IMessagePackFormatter[]{new StatusCodeFormatter()},new IFormatterResolver[]{ContractlessStandardResolver.Instance}));
            
        }
        public override T Deserialize<T>(ArraySegment<byte> rawBytes)
        {
            return MessagePackSerializer.Deserialize<T>(rawBytes,_options);
        }

        public override byte[] Serialize<T>(T obj)
        {
            return MessagePackSerializer.Serialize(obj,_options);
        }

        public override async Task<T> DeserializeAsync<T>(ArraySegment<byte> rawBytes)
        {
            using (var memory = new MemoryStream(rawBytes.ToArray()))
            {
                return await MessagePackSerializer.DeserializeAsync<T>(memory,_options);
            }
        }

        public override async Task<byte[]> SerializeAsync<T>(T obj)
        {
            using (var memory = new MemoryStream())
            {
                await MessagePackSerializer.SerializeAsync(memory, obj,_options);
                return memory.ToArray();
            }
        }
    }
}