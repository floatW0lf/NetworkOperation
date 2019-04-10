using System;
using System.Threading.Tasks;
using Contract;
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
            
            var server = kernel.Get<IHost>();
            server.Start(Port);
            server.Sessions.OnSessionOpened += session => Console.WriteLine($"Session Opened {session.NetworkAddress} {session["appid"]}");
            server.Sessions.OnSessionClosed += session => Console.WriteLine($"Session Closed {session.NetworkAddress} {session.GetReason()}" );

            Console.WriteLine("Server started");

            Console.ReadLine();

            Console.WriteLine("Send message");
            var msg = Console.ReadLine();
            await server.Executor.Execute<ClientOp, Empty>(new ClientOp() { Message = msg });

            Console.ReadLine();
            await server.ShutdownAsync();
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
