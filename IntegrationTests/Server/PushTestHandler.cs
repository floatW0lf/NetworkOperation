using System.Threading;
using System.Threading.Tasks;
using IntegrationTests.Contract;
using NetworkOperation.Core;
using NetworkOperation.Core.Messages;
using NetworkOperation.Core.Models;
using NetworkOperation.Host;

namespace IntegrationTests.Server
{
    public class PushTestHandler : IHandler<PushTest,string,DefaultMessage>
    {
        private readonly IHostContext _host;

        public PushTestHandler(IHostContext host)
        {
            _host = host;
        }
            
        public async Task<OperationResult<string>> Handle(PushTest objectData, RequestContext<DefaultMessage> context, CancellationToken token)
        {
            var result = await _host.Executor.Execute(new ClientOp() {Message = objectData.Message + "_server"},o => o);
            return this.Return(result.Result);
        }
    }
}
