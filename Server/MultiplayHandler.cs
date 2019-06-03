using System;
using System.Threading;
using System.Threading.Tasks;
using Contract;
using NetworkOperation;
using NetworkOperation.Extensions;
using NetworkOperation.Server;

namespace Server
{
    public class MultiplayHandler : IHandler<Multiplay,float,DefaultMessage>
    {
        private readonly IHost _host;

        public MultiplayHandler(IHost host)
        {
            _host = host;
        }

        public async Task<OperationResult<float>> Handle(Multiplay objectData, RequestContext<DefaultMessage> context, CancellationToken token)
        {
            Console.WriteLine("Multiplay Handle");
            await _host.Executor.Execute<ClientOp, Empty>(new ClientOp() {Message = "multyply"});
            return this.Return(objectData.A * objectData.B);
        }
    }
}