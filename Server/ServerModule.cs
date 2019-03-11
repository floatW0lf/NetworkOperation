using System;
using System.Net.Sockets;
using LiteNetLib;
using NetLibOperation;
using NetOperationTest;
using NetworkOperation.Dispatching;
using NetworkOperation.Factories;
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
            Bind<OperationRuntimeModel>().ToConstant(OperationRuntimeModel.CreateFromAttribute());
            Bind<IFactory<NetPeer, Session>>().To<SessionFactory>().InSingletonScope();
            Bind<IFactory<Socket, Session>>().To<Tcp.Core.SessionFactory>().InSingletonScope();
            Bind<IFactory<Socket, MutableSessionCollection>>().To<Tcp.Server.SessionsFactory>().InSingletonScope();
            Bind<IFactory<NetManager, MutableSessionCollection>>().To<NetLibOperation.SessionsFactory>().InSingletonScope();
            
            Bind<IFactory<SessionCollection, IServerOperationExecutor>>()
                .To<DefaultServerOperationExecutorFactory<DefaultMessage,DefaultMessage>>().InSingletonScope();
            

            Bind<BaseSerializer>().To<MsgSerializer>().InSingletonScope();
            Bind<BaseDispatcher<DefaultMessage,DefaultMessage>>().To<ExpressionDispatcher<DefaultMessage,DefaultMessage>>().InSingletonScope();
            Bind<IRequestPlaceHolder<DefaultMessage>>().ToConstant(NullRequestPlaceHolder<DefaultMessage>.Instance);
            Bind<IResponsePlaceHolder<DefaultMessage, DefaultMessage>>()
                .ToConstant(NullResponsePlaceHolder<DefaultMessage, DefaultMessage>.Instance);
            
            if (_useTcp)
            {
                Bind<IServer>().To<TcpNetOperationServer<DefaultMessage,DefaultMessage>>().InSingletonScope();
            }
            else
            {
                Bind<string>().ToConstant("key").WhenInjectedInto<Server<DefaultMessage,DefaultMessage>>();
                Bind<IServer>().To<Server<DefaultMessage,DefaultMessage>>().InSingletonScope();
            }
            
            Bind<IHandlerFactory>().To<NinjectHandlerFactory>().InSingletonScope();

            Kernel.Bind(x => x.From(AppDomain.CurrentDomain.GetAssemblies()).SelectAllClasses().InheritedFrom<IHandler>().BindAllInterfaces().Configure((syntax, type) => syntax.InTransientScope()));
            
            
        }
    }
}