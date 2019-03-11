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
        private readonly IServer _server;

        public MultiplayHandler(IServer server)
        {
            _server = server;
        }

        public async Task<OperationResult<float>> Handle(Multiplay objectData, OperationContext<DefaultMessage> context, CancellationToken token)
        {
            Console.WriteLine("Multiplay Handle");
            await _server.Executor.Execute<ClientOp, Empty>(new ClientOp() {Message = "multyply"});
            return this.Return(objectData.A * objectData.B);
        }
    }
}