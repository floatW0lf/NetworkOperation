using System;
using System.Collections.Async;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Contract;
using NetLibOperation.Client;
using NetOperationTest;
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
            await WithBuilder();
            await WithDI();
        }

        private static async Task WithBuilder()
        {
            var client = new NetLibClientBuilder<DefaultMessage, DefaultMessage>().UseConnectKey("key").UseSerializer(new MsgSerializer()).Register(typeof(ClientOpHandler)).Build();
            
            await client.ConnectAsync("localhost", Port);
            var res = await client.Executor.Execute<PlusOp, float>(new PlusOp {A = 100, B = 200});
            Console.WriteLine(res.Result);
            await Task.Delay(2000);
            await client.DisconnectAsync();
        }
        private static async Task WithDI()
        {
            var useTcp = Read_YesNo("Use TCP ?");
            var k = new StandardKernel(new ClientModule(useTcp));
            var client = k.Get<IClient>();
            var evtsession = k.Get<ISessionEvents>();
            evtsession.OnSessionOpened += session => Console.WriteLine($"Session Opened {session.NetworkAddress}");
            evtsession.OnSessionClosed += session => Console.WriteLine($"Session Closed {session.NetworkAddress}");

            await client.ConnectAsync("localhost", Port);
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));

            var res = await client.Executor.Execute<PlusOp, float>(new PlusOp {A = 100, B = 200});
            res.Handle().Success(f => { }).BuiltInCode(BuiltInOperationState.InternalError, () => { });
            Console.WriteLine(res);

            try
            {
                var result = await client.Executor.Execute<LongTimeOperation, int>(new LongTimeOperation(), cts.Token);
                Console.WriteLine($"Result : {result}");
            }
            catch (TaskCanceledException e)
            {
                Console.WriteLine(e);
            }
            await client.DisconnectAsync();
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

