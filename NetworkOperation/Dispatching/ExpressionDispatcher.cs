using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetworkOperation.Core.Messages;
using NetworkOperation.Core.Models;

namespace NetworkOperation.Core.Dispatching
{
    public sealed class ExpressionDispatcher<TRequest, TResponse> : BaseDispatcher<TRequest, TResponse>
        where TRequest : IOperationMessage, new() where TResponse : IOperationMessage, new()
    {
        private delegate Task<DataWithStateCode> DispatchDelegate(ExpressionDispatcher<TRequest, TResponse> @this,
            TRequest header, RequestContext<TRequest> context, CancellationToken token);
        
        private static readonly MethodInfo GenericHandleMethod =
            typeof(BaseDispatcher<TRequest, TResponse>).GetMethod("GenericHandle",
                BindingFlags.Instance | BindingFlags.NonPublic);

        private DispatchDelegate _cacheDispatcher;

        public ExpressionDispatcher(BaseSerializer serializer, IHandlerFactory factory, OperationRuntimeModel model, ILoggerFactory logger, DescriptionRuntimeModel descriptionRuntimeModel) : base(serializer, factory, model, logger, descriptionRuntimeModel)
        {
        }

        private DispatchDelegate GenerateMethod(Side currentSide)
        {
            var thisRef = Expression.Parameter(GetType(), "@this");
            var header = Expression.Parameter(typeof(TRequest), "header");
            var context = Expression.Parameter(typeof(RequestContext<TRequest>), "ctx");
            var cancelToken = Expression.Parameter(typeof(CancellationToken), "token");

            var cases = Model.Where(d => d != null && d.Handle.HasFlag(currentSide)).Select(d =>
            {
                var method = GenericHandleMethod.MakeGenericMethod(d.OperationType, d.ResultType);
                return Expression.SwitchCase(
                    Expression.Call(thisRef, method,  header, context, cancelToken),
                    Expression.Constant(d.Code, d.Code.GetType()));
            }).ToArray();

            var switchExpression = Expression.Switch(
                Expression.Property(header, nameof(IOperationMessage.OperationCode)),
                Expression.Throw(Expression.New(typeof(InvalidOperationException)), typeof(Task<DataWithStateCode>)),
                cases);

            var lambda = Expression.Lambda<DispatchDelegate>(switchExpression, thisRef,  header, context,cancelToken);

            return lambda.Compile();
        }

        protected override Task<DataWithStateCode> ProcessHandler(TRequest header, RequestContext<TRequest> context, CancellationToken token)
        {
            _cacheDispatcher = _cacheDispatcher ?? GenerateMethod(ExecutionSide);
            return _cacheDispatcher(this, header, context, token);
        }
    }
}