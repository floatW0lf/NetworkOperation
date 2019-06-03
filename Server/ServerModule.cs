using System;
using System.Net.Sockets;
using LiteNetLib;
using Microsoft.Extensions.Hosting;
using NetLibOperation;
using NetOperationTest;
using NetworkOperation.Dispatching;
using NetworkOperation.Factories;
using NetworkOperation.Host;
using NetworkOperation.Logger;
using NetworkOperation.Server;
using Ninject.Extensions.Conventions;
using Ninject.Modules;
using Server;
using Tcp.Server;

namespace NetworkOperation
{
    public class ServerModule : NinjectModule
    {
        private readonly bool _useTcp;

        public ServerModule(bool useTcp)
        {
            _useTcp = useTcp;
        }
        public override void Load()
        {
            Bind<SessionRequestHandler>().To<ExampleSessionRequestHandler>().InSingletonScope();
            Bind<OperationRuntimeModel>().ToConstant(OperationRuntimeModel.CreateFromAttribute());
            Bind<IFactory<NetPeer, Session>>().To<SessionFactory>().InSingletonScope();
            Bind<IFactory<Socket, Session>>().To<Tcp.Core.SessionFactory>().InSingletonScope();
            Bind<IFactory<Socket, MutableSessionCollection>>().To<Tcp.Server.SessionsFactory>().InSingletonScope();
            Bind<IFactory<NetManager, MutableSessionCollection>>().To<NetLibOperation.SessionsFactory>().InSingletonScope();
            Bind<IStructuralLogger>().To<ConsoleStructuralLogger>().InSingletonScope();
            
            Bind<IFactory<SessionCollection, IHostOperationExecutor>>()
                .To<DefaultServerOperationExecutorFactory<DefaultMessage,DefaultMessage>>().InSingletonScope();
            

            Bind<BaseSerializer>().To<MsgSerializer>().InSingletonScope();
            Bind<BaseDispatcher<DefaultMessage,DefaultMessage>>().To<ExpressionDispatcher<DefaultMessage,DefaultMessage>>().InSingletonScope();
            
            if (_useTcp)
            {
                Bind<IHostContext, IHostedService>().To<TcpNetOperationHost<DefaultMessage, DefaultMessage>>().InSingletonScope();
            }
            else
            {
                Bind<string>().ToConstant("key").WhenInjectedInto<NetLibHost<DefaultMessage,DefaultMessage>>();
                Bind<IHostContext,IHostedService>().To<NetLibHost<DefaultMessage,DefaultMessage>>().InSingletonScope().OnActivation(host => host.ListenPort = 9050);
            }
            
            Bind<IHandlerFactory>().To<NinjectHandlerFactory>().InSingletonScope();

            Kernel.Bind(x => x.From(AppDomain.CurrentDomain.GetAssemblies()).SelectAllClasses().InheritedFrom<IHandler>().BindAllInterfaces().Configure((syntax, type) => syntax.InTransientScope()));
            
            
        }
    }
}