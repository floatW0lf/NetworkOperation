using NetOperationTest;
using NetworkOperation;
using NetworkOperation.Client;
using NetworkOperation.Dispatching;
using Ninject.Extensions.Conventions;
using Ninject.Modules;
using System;
using System.Net.Sockets;
using LiteNetLib;
using NetLibOperation;
using NetworkOperation.Factories;
using Tcp.Client;

namespace Client
{
    public class ClientModule : NinjectModule
    {
        private readonly bool _useTcp;

        public ClientModule(bool useTcp)
        {
            _useTcp = useTcp;
        }
        public override void Load()
        {
            Bind<string>().ToConstant("key").WhenInjectedInto<NetLibOperation.Client.Client<DefaultMessage,DefaultMessage>>();
            Bind<OperationRuntimeModel>().ToConstant(OperationRuntimeModel.CreateFromAttribute());

            Bind<BaseSerializer>().To<MsgSerializer>().InSingletonScope();
            Bind<BaseDispatcher<DefaultMessage,DefaultMessage>>().To<ExpressionDispatcher<DefaultMessage,DefaultMessage>>().InSingletonScope();
            Bind<IFactory<NetPeer, Session>>().To<SessionFactory>().InSingletonScope();
            Bind<IFactory<Socket, Session>>().To<Tcp.Core.SessionFactory>().InSingletonScope();
            Bind<IFactory<Session, IClientOperationExecutor>>().To<DefaultClientOperationExecutorFactory<DefaultMessage,DefaultMessage>>().InSingletonScope();
            
            Bind<IHandlerFactory>().To<DefaultHandlerFactory>();
            
            if (_useTcp)
            {
                Bind<IClient,ISessionEvents>().To<TcpNetOperationClient<DefaultMessage,DefaultMessage>>().InSingletonScope();
            }
            else
            {
                Bind<IClient,ISessionEvents>().To<NetLibOperation.Client.Client<DefaultMessage,DefaultMessage>>().InSingletonScope();
            }
            
            Kernel.Bind(x => x.From(AppDomain.CurrentDomain.GetAssemblies()).SelectAllClasses().InheritedFrom<IHandler>().BindAllInterfaces().Configure((syntax, type) => syntax.InTransientScope()));
        }
    }
}

