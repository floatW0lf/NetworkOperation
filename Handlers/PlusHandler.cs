using System;
using System.Threading.Tasks;
using Contract;
using NetworkOperation;

namespace Handlers
{
    public class PlusHandler : IHandler<PlusOp,float>
    {
        public async Task<float> Handle(PlusOp objectData)
        {
            return await Task.Run(() => objectData.A + objectData.B);
        }
    }
}
