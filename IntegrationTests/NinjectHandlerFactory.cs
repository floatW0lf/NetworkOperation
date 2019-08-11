using NetworkOperation;
using Ninject;

namespace IntegrationTests
{
    public class NinjectHandlerFactory : IHandlerFactory
    {
        private readonly IKernel _kernel;

        public NinjectHandlerFactory(IKernel kernel)
        {
            _kernel = kernel;
        }

        public IHandler<TOperation, TResult, TRequest> Create<TOperation, TResult, TRequest>() where TOperation : IOperation<TOperation, TResult> where TRequest : IOperationMessage
        {
            return _kernel.Get<IHandler<TOperation, TResult, TRequest>>();
        }

        public void Destroy(IHandler handler)
        {
            _kernel.Release(handler);
        }
    }
}