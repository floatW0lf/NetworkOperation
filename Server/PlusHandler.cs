using System;
using System.Threading;
using System.Threading.Tasks;
using Contract;
using NetworkOperation;
using NetworkOperation.Extensions;
using NetworkOperation.Server;

namespace Handlers
{
    public class PlusHandler : IHandler<PlusOp,float,DefaultMessage>
    {
        private readonly IServer _server;

        public PlusHandler(IServer server)
        {
            _server = server;
        }
            
        public async Task<OperationResult<float>> Handle(PlusOp objectData, RequestContext<DefaultMessage> context, CancellationToken token)
        {
            //_server.Shutdown();
            return this.Return(objectData.A + objectData.B);
        }
    }
}
