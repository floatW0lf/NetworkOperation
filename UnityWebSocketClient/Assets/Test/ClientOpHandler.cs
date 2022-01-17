using System.Threading;
using System.Threading.Tasks;
using NetworkOperation.Core;
using NetworkOperation.Core.Messages;
using NetworkOperation.Core.Models;
using UnityEngine;

namespace WebGL.WebSockets.Tests
{
    public class ClientOpHandler : IHandler<ClientOp,Empty,DefaultMessage>
    {
        public async Task<OperationResult<Empty>> Handle(ClientOp objectData, RequestContext<DefaultMessage> context, CancellationToken token)
        {
            Debug.Log(objectData.Message);
            return this.ReturnEmpty();
        }
    }

    public class LargeDataHandler : IHandler<LargeDataOperation, int, DefaultMessage>
    {
        public async Task<OperationResult<int>> Handle(LargeDataOperation objectData, RequestContext<DefaultMessage> context, CancellationToken token)
        {
            Debug.Log($"Receive data {objectData.Raw.Length}");
            return this.Return(objectData.Raw.Length);
        }
        
    }
}