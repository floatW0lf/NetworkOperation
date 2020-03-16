using MessagePack;
using MessagePack.Formatters;
using NetworkOperation;

namespace Serializer.MessagePack
{
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