using System;
using System.Threading;
using System.Threading.Tasks;
using Contract;
using NetworkOperation;
using NetworkOperation.Extensions;

namespace Handlers
{
    public class PlusHandler : IHandler<PlusOp,float,DefaultMessage>
    {
        public async Task<OperationResult<float>> Handle(PlusOp objectData, OperationContext<DefaultMessage> context, CancellationToken token)
        {
            return this.Return(objectData.A + objectData.B);
        }
    }
}
