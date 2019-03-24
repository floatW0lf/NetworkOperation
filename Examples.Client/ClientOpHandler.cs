using System;
using System.Threading;
using System.Threading.Tasks;
using Contract;
using NetworkOperation;
using NetworkOperation.Extensions;

namespace Client
{
    public class ClientOpHandler : IHandler<ClientOp,Empty,DefaultMessage>
    {
        public async Task<OperationResult<Empty>> Handle(ClientOp objectData, RequestContext<DefaultMessage> context, CancellationToken token)
        {
            Console.WriteLine(objectData.Message);
            return this.Return(default);
        }
    }
}