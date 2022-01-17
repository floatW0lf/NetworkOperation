using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
        private ILogger<Op2Handler> _logger;

        public Op2Handler(ILogger<Op2Handler> logger)
        {
            _logger = logger;
        }
        public async Task<OperationResult<string>> Handle(TestOp2 objectData, RequestContext<DefaultMessage> context, CancellationToken token)
        {
            _logger.LogInformation("Receive message {message}", objectData.Message);
            return this.Return(objectData.Message + "-pong");
        }
    }

    public class LargeData : IHandler<LargeDataOperation, int, DefaultMessage>
    {
        private ILogger<LargeData> _logger;

        public LargeData(ILogger<LargeData> logger)
        {
            _logger = logger;
        }
        public async Task<OperationResult<int>> Handle(LargeDataOperation objectData, RequestContext<DefaultMessage> context, CancellationToken token)
        {
            _logger.LogInformation("Receive large data {count}", objectData.Raw.Length);
            return this.Return(objectData.Raw.Length);
        }
    }
}