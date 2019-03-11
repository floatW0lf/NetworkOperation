using System.Runtime.Serialization;

namespace NetworkOperation
{
    [DataContract]
    public struct Empty
    {
        public static readonly Empty value = new Empty();
    }
}