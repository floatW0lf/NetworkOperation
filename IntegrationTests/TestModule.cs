using System;
using System.Linq;
using System.Net.Sockets;
using LiteNetLib;
using Microsoft.Extensions.Hosting;
using Moq;
using NetLibOperation;
using NetworkOperation;
using NetworkOperation.Client;
using NetworkOperation.Dispatching;
using NetworkOperation.Factories;
using NetworkOperation.Host;
using NetworkOperation.Logger;
using NetworkOperation.Server;
using Ninject.Extensions.Conventions;
using Ninject.Modules;
using Serializer.MessagePack;
using Tcp.Client;
using Tcp.Server;

namespace IntegrationTests.Client
{
    public class TestModule : NinjectModule
    {
        private readonly bool _useTcp;

        public TestModule(bool useTcp)
        {
            _useTcp = useTcp;
        }

        public override void Load()
        {
            
            Bind<OperationRuntimeModel>().ToConstant(OperationRuntimeModel.CreateFromAttribute());

            Bind<IStructuralLogger>()
                .ToMethod(context => new ConsoleStructuralLogger(context.Request.Target.Member.DeclaringType.FullName))
                .InTransientScope();


            Bind<BaseSerializer>().To<MsgSerializer>().InSingletonScope();
            Bind<BaseDispatcher<DefaultMessage, DefaultMessage>>()
                .To<ExpressionDispatcher<DefaultMessage, DefaultMessage>>().InTransientScope();

            Bind<SessionRequestHandler>().To<DefaultLiteSessionOpenHandler>().InSingletonScope();
            
            Bind<IFactory<NetPeer, Session>>().To<SessionFactory>().InSingletonScope();
            Bind<IFactory<Socket, Session>>().To<Tcp.Core.SessionFactory>().InSingletonScope();
            Bind<IFactory<Session, IClientOperationExecutor>>()
                .To<DefaultClientOperationExecutorFactory<DefaultMessage, DefaultMessage>>().InSingletonScope();

            Bind<IFactory<NetManager, MutableSessionCollection>>().To<NetLibOperation.SessionsFactory>().InSingletonScope();
            Bind<IFactory<SessionCollection, IHostOperationExecutor>>().To<DefaultServerOperationExecutorFactory<DefaultMessage,DefaultMessage>>().InSingletonScope();
            Bind<IHandlerFactory>().To<NinjectHandlerFactory>();

            Bind<ILoggerFactory>().ToMethod(context =>
            {
                var m = new Mock<ILoggerFactory>();
                m.Setup(factory => factory.Create(It.IsAny<string>())).Returns(new ConsoleStructuralLogger());
                return m.Object;
            }).InSingletonScope();

            if (_useTcp)
            {
                Bind<IClient, ISessionEvents>().To<TcpNetOperationClient<DefaultMessage, DefaultMessage>>()
                    .InSingletonScope();
                Bind<IHostContext, IHostedService>().To<TcpNetOperationHost<DefaultMessage, DefaultMessage>>()
                    .InSingletonScope();
            }
            else
            {
                Bind<IHostContext, IHostedService>().To<NetLibHost<DefaultMessage, DefaultMessage>>().InSingletonScope();
                Bind<IClient, ISessionEvents>().To<NetLibOperation.Client.Client<DefaultMessage, DefaultMessage>>()
                    .InSingletonScope();
            }

            Kernel.Bind(x =>
                x.From(AppDomain.CurrentDomain.GetAssemblies().Where(assembly => !assembly.IsDynamic))
                    .SelectAllClasses().InheritedFrom<IHandler>().BindAllInterfaces()
                    .Configure((syntax, type) => syntax.InTransientScope()));
        }
    }
}