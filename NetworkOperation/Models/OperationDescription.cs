using System;

namespace NetworkOperation
{
    public class OperationDescription
    {
        public OperationDescription(uint code, Type operationType, Type resultType, Side handle, DeliveryMode forRequest, DeliveryMode forResponse, bool waitResponse)
        {
            Code = code;
            OperationType = operationType;
            ResultType = resultType;
            Handle = handle;
            ForRequest = forRequest;
            ForResponse = forResponse;
            WaitResponse = waitResponse;
        }

        public Side Handle { get; }
        public bool UseAsyncSerialize { get; set; }
        public uint Code { get; }
        public Type OperationType { get; }
        public Type ResultType { get; }
        
        public DeliveryMode ForRequest { get; }
        public DeliveryMode ForResponse { get; }

        public bool WaitResponse { get; }
    }
}