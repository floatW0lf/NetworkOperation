using System.Threading;
using System.Threading.Tasks;
using IntegrationTests.Contract;
using NetworkOperation;
using NetworkOperation.Extensions;
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
            return this.Return(objectData.A * objectData.B);
        }
    }
}