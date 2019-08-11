using System;
using System.Threading;
using System.Threading.Tasks;
using IntegrationTests.Contract;
using NetworkOperation;
using NetworkOperation.Extensions;

namespace IntegrationTests.Client
{
    public class ClientOpHandler : IHandler<ClientOp,Empty,DefaultMessage>
    {
        public async Task<OperationResult<Empty>> Handle(ClientOp objectData, RequestContext<DefaultMessage> context, CancellationToken token)
        {
            return this.Return(default);
        }
    }
}