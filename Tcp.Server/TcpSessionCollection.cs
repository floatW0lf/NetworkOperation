using NetworkOperation;

namespace Tcp.Server
{
    public class TcpSessionCollection : MutableSessionCollection
    {
        public override NetworkStatistics Statistics { get; }
    }
}