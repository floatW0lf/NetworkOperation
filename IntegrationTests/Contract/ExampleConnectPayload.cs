using System.Runtime.Serialization;
using NetworkOperation;

namespace IntegrationTests.Contract
{
    [DataContract]
    public struct ExampleConnectPayload : IConnectPayload
    {
        [DataMember]
        public string Authorize { get; set; }
        [DataMember]
        public string AppId { get; set; }
        [DataMember]
        public string Version { get; set; }
    }
}