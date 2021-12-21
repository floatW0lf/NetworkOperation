using System;
using System.Buffers;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using NetworkOperation.Core;
using NetworkOperation.Core.Dispatching;

namespace Serializer.MessagePack
{
    public class MsgSerializer : BaseSerializer
    {
        private static MessagePackSerializerOptions _options;
        static MsgSerializer()
        {
            _options = MessagePackSerializerOptions.Standard.WithResolver(CompositeResolver.Create(new IMessagePackFormatter[]{new StatusCodeFormatter()},new IFormatterResolver[]{ContractlessStandardResolver.Instance}));
        }

        public override TypeMessage ReadMessageType(ReadOnlyMemory<byte> rawBytes)
        {
            return MessagePackSerializer.Deserialize<TypeMessage>(rawBytes.Slice(1, 1), _options);
        }

        public override T Deserialize<T>(ReadOnlyMemory<byte> rawBytes, Session context)
        {
            
            return MessagePackSerializer.Deserialize<T>(rawBytes,_options);
        }

        public override ReadOnlyMemory<byte> Serialize<T>(T obj,Session context)
        {
            return MessagePackSerializer.Serialize(obj,_options);
        }

        public override async Task<T> DeserializeAsync<T>(ReadOnlyMemory<byte> rawBytes, Session context)
        {
            using (var memory = new MemoryStream(rawBytes.ToArray()))
            {
                return await MessagePackSerializer.DeserializeAsync<T>(memory,_options);
            }
        }

        public override async Task<ReadOnlyMemory<byte>> SerializeAsync<T>(T obj, Session context)
        {
            using (var memory = new MemoryStream())
            {
                await MessagePackSerializer.SerializeAsync(memory, obj,_options);
                return memory.ToArray();
            }
        }
    }
}