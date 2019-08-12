using System.Runtime.Serialization;
using NetworkOperation;

namespace IntegrationTests.Contract
{
    [DataContract]
    [Operation(3)]
    public struct LongTimeOperation : IOperation<LongTimeOperation,int>
    {
    }
}