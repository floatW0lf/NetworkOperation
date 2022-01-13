using System;
using System.Threading;
using System.Threading.Tasks;
using NetworkOperation.Core;
using NetworkOperation.Core.Messages;
using NetworkOperation.Core.Models;
using WebGL.WebSockets.Tests;

namespace WebSocketTest
{
    public class OpHandler : IHandler<TestOp,int,DefaultMessage>
    {
        public async Task<OperationResult<int>> Handle(TestOp objectData, RequestContext<DefaultMessage> context, CancellationToken token)
        {
            return this.Return(1);
        }
    }

    public class Op2Handler : IHandler<TestOp2, string, DefaultMessage>
    {
        public async Task<OperationResult<string>> Handle(TestOp2 objectData, RequestContext<DefaultMessage> context, CancellationToken token)
        {
            Console.WriteLine(objectData.Message);
            return this.Return(objectData.Message + "-pong");
        }
    }
}