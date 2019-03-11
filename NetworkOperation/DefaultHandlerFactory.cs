using System;
using System.Linq;

namespace NetworkOperation
{
    public class DefaultHandlerFactory : IHandlerFactory
    {
        public IHandler<TOp, TResult, TMessage> Create<TOp, TResult, TMessage>() where TOp : IOperation<TOp, TResult> where TMessage : IOperationMessage
        {
            var handlerType = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes()).First(type =>
                typeof(IHandler<TOp, TResult,TMessage>).IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface);

            return (IHandler<TOp, TResult, TMessage>) Activator.CreateInstance(handlerType);
        }

        public void Destroy(IHandler handler)
        {
            
        }
    }
}