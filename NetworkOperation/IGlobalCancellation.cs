using System.Threading;

namespace NetworkOperation
{
    public interface IGlobalCancellation
    {
        CancellationToken GlobalToken { get; set; }
    }
}