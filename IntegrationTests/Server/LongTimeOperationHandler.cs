using System;
using System.Threading;
using System.Threading.Tasks;
using Contract;
using NetworkOperation;

namespace Server
{
    public class LongTimeOperationHandler : IHandler<LongTimeOperation, int, DefaultMessage>
    {
        public async Task<OperationResult<int>> Handle(LongTimeOperation objectData,
            RequestContext<DefaultMessage> context, CancellationToken token)
        {
            Console.WriteLine("LongTimeOperationHandler");
            try
            {
                while (true)
                {
                    token.ThrowIfCancellationRequested();
                    await Task.Delay(10, token);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            return new OperationResult<int>();
        }
    }
}
