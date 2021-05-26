using System.Runtime.Serialization;

namespace NetworkOperation.Core
{
    [DataContract]
    public struct EmptyPayload : IConnectPayload { }
}