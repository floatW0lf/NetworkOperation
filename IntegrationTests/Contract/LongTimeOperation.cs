using System.Runtime.Serialization;
using NetworkOperation.Core;

namespace IntegrationTests.Contract
{
    [DataContract]
    [Operation(3)]
    public struct LongTimeOperation : IOperation<int>
    {
    }
}