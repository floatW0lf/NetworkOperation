using System;
using System.Threading;
using System.Threading.Tasks;
using IntegrationTests.Contract;
using NetworkOperation;
using NetworkOperation.Extensions;
using NetworkOperation.Server;

namespace IntegrationTests.Server
{
    public class PlusHandler : IHandler<PlusOp,float,DefaultMessage>
    {
        private readonly IHostContext _host;

        public PlusHandler(IHostContext host)
        {
            _host = host;
        }
            
        public async Task<OperationResult<float>> Handle(PlusOp objectData, RequestContext<DefaultMessage> context, CancellationToken token)
        {
            Console.WriteLine("Plus execute");
            Task.Delay(1000, token).ContinueWith(async task =>
            {
                await _host.Executor.Execute<ClientOp, Empty>(new ClientOp() {Message = "push from + operation"});
            },TaskContinuationOptions.NotOnCanceled).GetAwaiter();
            
            return this.Return(objectData.A + objectData.B);
        }
    }
}
