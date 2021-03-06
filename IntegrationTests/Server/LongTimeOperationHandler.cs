using System.Threading;
using System.Threading.Tasks;
using IntegrationTests.Contract;
using NetworkOperation.Core;
using NetworkOperation.Core.Messages;
using NetworkOperation.Core.Models;

namespace IntegrationTests.Server
{
    public class LongTimeOperationHandler : IHandler<LongTimeOperation, int, DefaultMessage>
    {
        public async Task<OperationResult<int>> Handle(LongTimeOperation objectData,
            RequestContext<DefaultMessage> context, CancellationToken token)
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();
                await Task.Delay(10, token);
            }
        }
    }
}
