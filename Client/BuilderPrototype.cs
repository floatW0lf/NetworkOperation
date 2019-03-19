using NetLibOperation;
using NetworkOperation;
using NetLibOperation.Client;
using NetOperationTest;
using NetworkOperation.Client;
using NetworkOperation.Dispatching;

namespace Client
{
    public class BuilderPrototype
    {
        public void CreateClient()
        {
            var handlerFactory = new DefaultHandlerFactory();
            var sessionFactory = new SessionFactory(handlerFactory);
            var model = OperationRuntimeModel.CreateFromAttribute();
            var serial = new MsgSerializer();
            
            var client = new Client<DefaultMessage, DefaultMessage>(
                sessionFactory,
                new DefaultClientOperationExecutorFactory<DefaultMessage,DefaultMessage>(serial, model),
                new ExpressionDispatcher<DefaultMessage, DefaultMessage>(serial,handlerFactory,model),
                ""
                );
        }
    }
}