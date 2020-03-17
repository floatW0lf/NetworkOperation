using System.Threading;

namespace NetworkOperation.Core
{
    public interface IGlobalCancellation
    {
        CancellationToken GlobalToken { get; set; }
    }
}