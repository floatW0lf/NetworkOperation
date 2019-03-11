using System.Net.Sockets;

namespace Tcp.Core
{
    public static class TcpNetOperationExtensions
    {
        public static bool IsConnected(this Socket socket)
        {
            return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
        }
    }
}