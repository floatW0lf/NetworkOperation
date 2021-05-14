using MessagePack;
using MessagePack.Formatters;
using NetworkOperation.Core;

namespace Serializer.MessagePack
{
    public class StatusCodeFormatter : IMessagePackFormatter<StatusCode>
    {
        public void Serialize(ref MessagePackWriter writer, StatusCode value, MessagePackSerializerOptions options)
        {
            writer.WriteUInt8(value.TypeTag);
            writer.WriteUInt16(value.EnumValue);
        }

        public StatusCode Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            return new StatusCode(reader.ReadByte(), reader.ReadUInt16());
        }
    }
}