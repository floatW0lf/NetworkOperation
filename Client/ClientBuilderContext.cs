using NetworkOperation.Logger;

namespace NetworkOperation.Client
{
    public class ClientBuilderContext
    {
        public IHandlerFactory HandlerFactory { get; set; } = new DefaultHandlerFactory();
        public BaseSerializer Serializer { get; set; }
        public OperationRuntimeModel Model { get; set; } = OperationRuntimeModel.CreateFromAttribute();
        public IStructuralLogger StructuralLogger { get; set; } = new ConsoleStructuralLogger();
    }
}