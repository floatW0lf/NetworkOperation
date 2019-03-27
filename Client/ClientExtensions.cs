using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkOperation.Client
{
    public static class ClientExtensions
    {
        public static async Task ConnectAsync(this IClient client, string address, int port, CancellationToken cancellationToken = default)
        {
            if (IPAddress.TryParse(address, out var ip)){}
            else
            {
               var addresses = await Dns.GetHostAddressesAsync(address);
               ip = addresses.FirstOrDefault();
            }
            if (ip == null) throw new ArgumentException($"{address} wrong address");
            await client.ConnectAsync(new IPEndPoint(ip, port), cancellationToken); 
            
        }
        
        public static async Task ConnectAsync(this IClient client, string addressWithPort, CancellationToken cancellationToken = default)
        {
            var strings = addressWithPort.Split(':');
            await client.ConnectAsync(strings[0], int.Parse(strings[1]), cancellationToken);
        }
    }
}