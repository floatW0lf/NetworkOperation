using System;

namespace NetworkOperation.Dispatching
{
    public enum TypeMessage
    {
        None,
        Request = 1,
        Response
    }
    
    public static class TypeMessageExtensions
    {
        public static ArraySegment<byte> ReadMessageType(this ArraySegment<byte> source, out TypeMessage type)
        {
            type = FromByte(source.Array[source.Offset]);
            return new ArraySegment<byte>(source.Array,source.Offset+1,source.Count-1);
        }

        public static ArraySegment<byte> AppendInBegin(this byte[] source, TypeMessage type)
        {
            var withType = new byte[source.Length + 1];
            withType[0] = ToByte(type);
            Array.Copy(source, 0, withType, 1, source.Length);
            return new ArraySegment<byte>(withType);
        }

        private static byte ToByte(TypeMessage type)
        {   
            switch (type)
            {
                case TypeMessage.Request:
                    return byte.MaxValue;
                
                case TypeMessage.Response:
                    return byte.MinValue;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private static TypeMessage FromByte(byte type)
        {
            switch (type)
            {
                case byte.MaxValue:
                    return TypeMessage.Request;
                    
                case byte.MinValue:
                    return TypeMessage.Response;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);  
            }
        }
    }
}