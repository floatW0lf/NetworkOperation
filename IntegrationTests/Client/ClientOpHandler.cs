using System.Threading;
using System.Threading.Tasks;
using IntegrationTests.Contract;
using NetworkOperation.Core;
using NetworkOperation.Core.Messages;
using NetworkOperation.Core.Models;

namespace IntegrationTests.Client
{
    public class ClientOpHandler : IHandler<ClientOp,string,DefaultMessage>
    {
        public async Task<OperationResult<string>> Handle(ClientOp objectData, RequestContext<DefaultMessage> context, CancellationToken token)
        {
            return this.Return(objectData.Message + "_client");
        }
    }
}