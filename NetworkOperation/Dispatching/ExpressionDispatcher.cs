using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;


namespace NetworkOperation.Dispatching
{
    public sealed class ExpressionDispatcher<TRequest, TResponse> : BaseDispatcher<TRequest, TResponse>
        where TRequest : IOperationMessage, new() where TResponse : IOperationMessage, new()
    {
        private static readonly MethodInfo GenericHandleMethod =
            typeof(BaseDispatcher<TRequest, TResponse>).GetMethod("GenericHandle",
                BindingFlags.Instance | BindingFlags.NonPublic);


        private DispatchDelegate _cacheDispatcher;

        public ExpressionDispatcher(BaseSerializer serializer, IHandlerFactory factory, OperationRuntimeModel model, ILoggerFactory logger) : base(serializer, factory, model, logger)
        {
        }


        private DispatchDelegate GenerateMethod(Side currentSide)
        {
            var thisRef = Expression.Parameter(GetType(), "@this");
            var session = Expression.Parameter(typeof(Session), "session");
            var message = Expression.Parameter(typeof(TRequest), "message");
            var description = Expression.Parameter(typeof(OperationDescription), "description");
            var cancelToken = Expression.Parameter(typeof(CancellationToken), "token");

            var cases = Model.Where(d => d != null && d.Handle.HasFlag(currentSide)).Select(d =>
            {
                var method = GenericHandleMethod.MakeGenericMethod(d.OperationType, d.ResultType);
                return Expression.SwitchCase(
                    Expression.Call(thisRef, method, session, message, description, cancelToken),
                    Expression.Constant(d.Code, d.Code.GetType()));
            }).ToArray();

            var switchExpression = Expression.Switch(
                Expression.Property(message, nameof(IOperationMessage.OperationCode)),
                Expression.Throw(Expression.New(typeof(InvalidOperationException)), typeof(Task<DataWithStateCode>)),
                cases);

            var lambda = Expression.Lambda<DispatchDelegate>(switchExpression, thisRef, session, message, description,
                cancelToken);

            return lambda.Compile();
        }

        protected override Task<DataWithStateCode> ProcessHandler(Session session, TRequest message,
            OperationDescription operationDescription, CancellationToken token)
        {
            _cacheDispatcher = _cacheDispatcher ?? GenerateMethod(ExecutionSide);
            return _cacheDispatcher(this, session, message, operationDescription, token);
        }

        private delegate Task<DataWithStateCode> DispatchDelegate(ExpressionDispatcher<TRequest, TResponse> @this,
            Session session, TRequest message, OperationDescription description, CancellationToken token);
    }
}