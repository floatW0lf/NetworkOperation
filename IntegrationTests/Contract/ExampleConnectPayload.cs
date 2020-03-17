using System.Runtime.Serialization;
using NetworkOperation.Core;

namespace IntegrationTests.Contract
{
    [DataContract]
    public struct ExampleConnectPayload : IConnectPayload
    {
        [DataMember(Order = 0)]
        public string Authorize { get; set; }
        [DataMember(Order = 1)]
        public string AppId { get; set; }
        [DataMember(Order = 2)]
        public string Version { get; set; }
    }
}