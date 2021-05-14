using System.Threading;
using System.Threading.Tasks;
using IntegrationTests.Contract;
using NetworkOperation.Core;
using NetworkOperation.Core.Messages;
using NetworkOperation.Core.Models;
using NetworkOperation.Host;

namespace IntegrationTests.Server
{
    public class MultiplayHandler : IHandler<Multiply,float,DefaultMessage>
    {
        private readonly IHostContext _host;

        public MultiplayHandler(IHostContext host)
        {
            _host = host;
        }

        public async Task<OperationResult<float>> Handle(Multiply objectData, RequestContext<DefaultMessage> context, CancellationToken token)
        {
            return this.ReturnCode(MultiplyStatus.OverFlow,t=>t,objectData.A * objectData.B);
        }
    }
}