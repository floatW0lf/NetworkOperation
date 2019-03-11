using System.Runtime.Serialization;
using NetworkOperation;

namespace Contract
{
    [DataContract]
    [Operation(3)]
    public struct LongTimeOperation : IOperation<LongTimeOperation,int>
    {
    }
}