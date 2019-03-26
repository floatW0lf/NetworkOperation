using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkOperation.Client
{
    public static class ClientExtensions
    {
        public static Task ConnectAsync(this IClient client, string address, int port, CancellationToken cancellationToken = default)
        {
            if (IPAddress.TryParse(address, out var ip)){}
            else
            {
                ip = Dns.GetHostAddresses(address).FirstOrDefault();
            }
            if (ip == null) throw new ArgumentException($"{address} wrong address");
            return client.ConnectAsync(new IPEndPoint(ip, port), cancellationToken); 
            
        }
        
        public static Task ConnectAsync(this IClient client, string addressWithPort, CancellationToken cancellationToken = default)
        {
            var strings = addressWithPort.Split(':');
            return client.ConnectAsync(strings[0], int.Parse(strings[1]), cancellationToken);
        }
    }
}