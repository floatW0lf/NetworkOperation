using System;
using System.Collections.Generic;
using System.Linq;

namespace NetworkOperation
{
    public class DefaultHandlerFactory : IHandlerFactory, IInterfaceMapAccessor
    {
        public Dictionary<Type,Type> InterfaceToClassMap { get; set; }
        public IHandler<TOp, TResult, TMessage> Create<TOp, TResult, TMessage>() where TOp : IOperation<TOp, TResult> where TMessage : IOperationMessage
        {
            var impl = InterfaceToClassMap[typeof(IHandler<TOp, TResult, TMessage>)];
            return (IHandler<TOp, TResult, TMessage>) Activator.CreateInstance(impl);
        }

        public void Destroy(IHandler handler)
        {
        }
    }
}