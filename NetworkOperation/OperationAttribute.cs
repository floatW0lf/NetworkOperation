using System;

namespace NetworkOperation
{
    public class OperationAttribute : Attribute
    {
        public OperationAttribute(uint code)
        {
            Code = code;
        }
        public uint Code { get; }
        public Side Handle { get; set; } = Side.All;
        public bool UseAsyncSerialize { get; set; }
        
    }
}