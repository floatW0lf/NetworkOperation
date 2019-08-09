using System;
using System.Threading;
using System.Threading.Tasks;
using Contract;
using Microsoft.Extensions.Hosting;
using NetLibOperation;
using NetworkOperation.Server;
using Ninject;


namespace NetworkOperation
{
    class Program
    {
        const int Port = 9050;
        
        static async Task Main(string[] args)
        {
            
            var useTcp = Read_YesNo("Use TCP ?");
            var kernel = new StandardKernel(new ServerModule(useTcp));
            
            var hostedService = kernel.Get<IHostedService>();
            await hostedService.StartAsync(CancellationToken.None);
            
            var hostOperation = kernel.Get<IHostContext>();
            hostOperation.Sessions.SessionOpened += session => Console.WriteLine($"Session Opened {session.NetworkAddress} {session["appid"]}");
            hostOperation.Sessions.SessionClosed += session => Console.WriteLine($"Session Closed {session.NetworkAddress} {session.GetReason()}" );

            Console.WriteLine("Server started");

            Console.ReadLine();

            Console.WriteLine("Send message");
            var msg = Console.ReadLine();
            await hostOperation.Executor.Execute<ClientOp, Empty>(new ClientOp() { Message = msg });

            Console.ReadLine();
            await hostedService.StopAsync(CancellationToken.None);
            Console.ReadLine();
        }

        public static bool Read_YesNo(string message)
        {
            Console.WriteLine($"{message} Y/N");
            switch (Console.ReadKey().Key)
            {
                case ConsoleKey.Y:
                    Console.WriteLine();
                    return true;
                case ConsoleKey.N:
                    Console.WriteLine();
                    return false;
            }
            Console.WriteLine();
            return Read_YesNo(message);
        }

    }
}
