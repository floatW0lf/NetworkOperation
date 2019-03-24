using System;
using System.Collections.Async;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Contract;
using NetworkOperation;
using NetworkOperation.Client;
using NetworkOperation.OperationResultHandler;
using Ninject;

namespace Client
{
    class Program
    {
        
        const int Port = 9050;
        
        static async Task Main(string[] args)
        {
            var useTcp = Read_YesNo("Use TCP ?");
            var k = new StandardKernel(new ClientModule(useTcp));
            var client = k.Get<IClient>();
            var evtsession = k.Get<ISessionEvents>();
            evtsession.OnSessionOpened += session => Console.WriteLine($"Session Opened {session.NetworkAddress}");
            evtsession.OnSessionClosed += session => Console.WriteLine($"Session Closed {session.NetworkAddress}");

            await client.ConnectAsync("localhost", Port);
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));

            var res = await client.Executor.Execute<PlusOp, float>(new PlusOp{A = 100, B = 200});
            res.Handle().Success(f => { }).BuiltInCode(BuiltInOperationState.InternalError, () => { });
            Console.WriteLine(res);
            //await client.DisconnectAsync();
            try
            {
                var result = await client.Executor.Execute<LongTimeOperation, int>(new LongTimeOperation(),cts.Token);
                Console.WriteLine($"Result : {result}");
            }
            catch (TaskCanceledException e)
            {
                Console.WriteLine(e);
            }
           
            var rnd = new Random();

            /*for (int i = 0; i < 10; i++)
            {
                var r = await client.Executor.Execute<Multiplay, float>(new Multiplay() { A = rnd.Next(), B = rnd.Next() });
                Console.WriteLine($"Result {r}");
                Console.WriteLine($"Call {i}");
                await Task.Delay(2000);
            }*/

            Console.ReadLine();
            await client.DisconnectAsync();
            Console.ReadLine();
            cts.Dispose();
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

