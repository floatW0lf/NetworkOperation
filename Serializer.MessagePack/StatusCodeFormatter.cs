using MessagePack;
using MessagePack.Formatters;
using NetworkOperation;

namespace Serializer.MessagePack
{
    public class StatusCodeFormatter : IMessagePackFormatter<StatusCode>
    {
        public int Serialize(ref byte[] bytes, int offset, StatusCode value, IFormatterResolver formatterResolver)
        {
            return MessagePackBinary.WriteUInt32(ref bytes, offset, value.Code);
        }

        public StatusCode Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            return (StatusCode) MessagePackBinary.ReadUInt32(bytes, offset, out readSize);
        }
    }
}