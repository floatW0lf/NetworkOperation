using System;

namespace NetworkOperation
{
    public class OperationDescription
    {
        public OperationDescription(uint code, Type operationType, Type resultType, Side handle)
        {
            Code = code;
            OperationType = operationType;
            ResultType = resultType;
            Handle = handle;
        }

        public Side Handle { get; }
        public bool UseAsyncSerialize { get; set; }
        public uint Code { get; }
        public Type OperationType { get; }
        public Type ResultType { get; }

        public bool WaitResponse { get; set; } = true;
    }
}