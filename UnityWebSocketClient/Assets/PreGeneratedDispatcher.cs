
using System;
using System.Threading.Tasks;
using System.Threading;
using NetworkOperation.Core;
using NetworkOperation.Core.Dispatching;
using NetworkOperation.Core.Models;
using NetworkOperation.Core.Messages;
using Microsoft.Extensions.Logging;


namespace NetworkOperations.Dispatching
{
    public class PreGeneratedDispatcher<TRequest, TResponse> : BaseDispatcher<TRequest, TResponse>
           where TRequest : IOperationMessage, new() where TResponse : IOperationMessage, new()
    {
        public PreGeneratedDispatcher(BaseSerializer serializer, IHandlerFactory factory, OperationRuntimeModel model, ILoggerFactory logger, DescriptionRuntimeModel descriptionRuntimeModel) : base(serializer, factory, model, logger,descriptionRuntimeModel)
        {
        }

        protected override Task<DataWithStateCode> ProcessHandler(TRequest message, RequestContext<TRequest> context, CancellationToken token)
        {
            switch (context.OperationDescription.Code)
            {
               case 2: return GenericHandle<WebGL.WebSockets.Tests.ClientOp, NetworkOperation.Core.Empty>(message, context, token);
            }
            throw new InvalidOperationException($"Wrong code operation {context.OperationDescription.Code}");
        }
    }
    
 
        public static partial class AOTSupport
        {
            public static void GeneratedDefinitions()
            {
                VirtualGenericMethodsDefinition<WebGL.WebSockets.Tests.TestOp>();                                        
                VirtualGenericMethodsDefinition<WebGL.WebSockets.Tests.TestOp2>();                                        
                VirtualGenericMethodsDefinition<WebGL.WebSockets.Tests.ClientOp>();                                        
            
                VirtualGenericMethodsDefinition<System.Int32>();                                       
                VirtualGenericMethodsDefinition<System.String>();                                       
                VirtualGenericMethodsDefinition<NetworkOperation.Core.Empty>();                                       
            }            
            static partial void VirtualGenericMethodsDefinition<T>();            
        }
 
}