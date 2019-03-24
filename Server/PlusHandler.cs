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
        private readonly IHost _host;

        public PlusHandler(IHost host)
        {
            _host = host;
        }
            
        public async Task<OperationResult<float>> Handle(PlusOp objectData, RequestContext<DefaultMessage> context, CancellationToken token)
        {
            //_server.Shutdown();
            return this.Return(objectData.A + objectData.B);
        }
    }
}
