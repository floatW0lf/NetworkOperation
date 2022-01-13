using NetworkOperation.Core.Messages;
using WebGL.WebSockets.Tests;

namespace NetworkOperations.Dispatching
{
    public partial class AOTSupport
    {
        static partial void VirtualGenericMethodsDefinition<T>()
        {
            var serializer = new MsgSerializer();
            serializer.Deserialize<T>(default, default);
            serializer.Serialize(default(T), default);
        }

        public static void Methods()
        {
            var dispatcher = new PreGeneratedDispatcher<DefaultMessage, DefaultMessage>(default,default,default,default,default);
            var serializer = new MsgSerializer();
            serializer.Serialize<ConnectPayload>(default, default);
            serializer.Deserialize<ConnectPayload>(default, default);
            serializer.Serialize<DisconnectPayload>(default, default);
            serializer.Deserialize<DisconnectPayload>(default, default);
        }
    }
}