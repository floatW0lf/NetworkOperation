using System;
using NetworkOperation.Core.Dispatching;

namespace NetworkOperation.Core.Messages
{
    public interface IStatus
    {
        StatusCode Status { get; set; }
    }

    public interface IOperationMessage : IStatus
    {
        TypeMessage Type { get; set; }
        int Id { get; set; }
        uint OperationCode { get; set; }
        ReadOnlyMemory<byte> OperationData { get; set; }
    }
}