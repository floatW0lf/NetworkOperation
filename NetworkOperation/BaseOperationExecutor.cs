using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NetworkOperation.Extensions;

namespace NetworkOperation
{
    public abstract class BaseOperationExecutor<TRequest, TResponse> : IResponseReceiver<TResponse> where TResponse : IOperationMessage, new() where TRequest : IOperationMessage, new()
    {
        private readonly ConcurrentDictionary<uint, Task> _responseQueue = new ConcurrentDictionary<uint, Task>();

        private readonly BaseSerializer _serializer;

        private readonly Pool<State> _states = new Pool<State>(() => new State(), 10, 100);
        private readonly Side _currentSide;
        private readonly Session _session;
        private readonly SessionCollection _sessions;
        private readonly IRequestPlaceHolder<TRequest> _messagePlaceHolder;

        protected BaseOperationExecutor(OperationRuntimeModel model, BaseSerializer serializer, SessionCollection sessions, IRequestPlaceHolder<TRequest> messagePlaceHolder)
        {
            Model = model;
            _serializer = serializer;
            _sessions = sessions;
            _messagePlaceHolder = messagePlaceHolder;
            _currentSide = Side.Server;
        }

        protected BaseOperationExecutor(OperationRuntimeModel model, BaseSerializer serializer, Session session, IRequestPlaceHolder<TRequest> messagePlaceHolder)
        {
            _messagePlaceHolder = messagePlaceHolder;
            Model = model;
            _serializer = serializer;
            _session = session;
            _currentSide = Side.Client;
        }


        protected OperationRuntimeModel Model { get; }

        bool IResponseReceiver<TResponse>.Receive(TResponse result)
        {
            if (result.StateCode != (uint)BuiltInOperationState.Handle && _responseQueue.TryRemove(result.OperationCode, out var task))
            {
                ((State) task.AsyncState).Result = result;
                task.Start();
                return true;
            }

            return false;
        }

        protected async Task<OperationResult<TResult>> SendOperation<TOp, TResult>(TOp operation, IReadOnlyList<Session> receivers,
            bool forAll, CancellationToken token) where TOp : IOperation<TOp, TResult>
        {
            
            var description = Model.GetDescriptionBy(typeof(TOp));
            var op = new TRequest()
            {
                OperationCode = description.Code,
                OperationData = description.UseAsyncSerialize
                    ? await _serializer.SerializeAsync(operation)
                    : _serializer.Serialize(operation)
            };
            
            _messagePlaceHolder.Fill(ref op, operation);
            
            token.Register(async
                (obj) =>
            {
                var desc = (OperationDescription)obj;
                await SendCancel(receivers, forAll, desc);
            }, description,false);

            var rawResult = _serializer.Serialize(op);
            await SendRaw(receivers, forAll, rawResult);
           

            try
            {
                var task = new Task<OperationResult<TResult>>(state =>
                {
                    var s = (State) state;
                    var message = s.Result;
                    _states.Put(s);
                    if (typeof(TResult) == typeof(Empty)) return new OperationResult<TResult>(default, message.StateCode);

                    return new OperationResult<TResult>(_serializer.Deserialize<TResult>(message.OperationData.To()), message.StateCode);
                }, _states.Rent(), token, TaskCreationOptions.PreferFairness);

                _responseQueue.TryAdd(description.Code, task);
                return await task;
            }
            catch (OperationCanceledException)
            {
                await SendCancel(receivers, forAll, description);
                throw;
            }
            
        }

        private async Task SendCancel(IReadOnlyList<Session> receivers, bool forAll, OperationDescription desc)
        {
            if (_responseQueue.TryRemove(desc.Code, out var canceledTask))
            {
                _states.Put((State) canceledTask.AsyncState);
                await SendRaw(receivers, forAll,
                    _serializer.Serialize(new TResponse {OperationCode = desc.Code, StateCode = (uint)BuiltInOperationState.Cancel}));
            }
        }

        private async Task SendRaw(IReadOnlyList<Session> receivers, bool forAll, byte[] rawResult)
        {
            switch (_currentSide)
            {
                case Side.Server when forAll:
                    await _sessions.SendToAllAsync(rawResult);
                    break;
                case Side.Server:
                {
                    for (var i = 0; i < receivers.Count; i++)
                    {
                        await receivers[i].SendMessageAsync(rawResult.To());
                    }
                    break;
                }
                case Side.Client:
                    await _session.SendMessageAsync(rawResult.To());
                    break;
            }
        }

        private class State
        {
            public TResponse Result;
        }
       
    }
}