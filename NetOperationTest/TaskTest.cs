using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace NetOperationTest
{
    public class TaskTest
    {
        [Fact]
        public void DisposeTaskTest()
        {
            var cts = new CancellationTokenSource();
            var task = new Task(() => { }, cts.Token);
            cts.Cancel();
            
            
        }
    }
}