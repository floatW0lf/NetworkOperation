using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NetworkOperation.Extensions;

namespace NetworkOperation
{
    public abstract class BaseOperationExecutor<TRequest, TResponse> : IResponseReceiver<TResponse>, IGlobalCancellation where TResponse : IOperationMessage, new() where TRequest : IOperationMessage, new()
         {
        private struct OperationId : IEquatable<OperationId>
        {
            public OperationId(int id, uint code)
            {
                Id = id;
                Code = code;
            }
            public bool Equals(OperationId other)
            {
                return Id == other.Id && Code == other.Code;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is OperationId other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (Id * 397) ^ (int) Code;
                }
            }

            public readonly int Id;
            public readonly uint Code;
            
        }
        
        private readonly ConcurrentDictionary<OperationId, Task> _responseQueue = new ConcurrentDictionary<OperationId, Task>();

        private readonly BaseSerializer _serializer;

        private readonly Pool<State> _states = new Pool<State>(() => new State(), 10, 100);
        private readonly Side _currentSide;
        private readonly Session _session;
        private readonly SessionCollection _sessions;
        

        public CancellationToken GlobalToken { get; set; }
        
        protected BaseOperationExecutor(OperationRuntimeModel model, BaseSerializer serializer, SessionCollection sessions)
        {
            Model = model;
            _serializer = serializer;
            _sessions = sessions;
            _currentSide = Side.Server;
        }

        protected BaseOperationExecutor(OperationRuntimeModel model, BaseSerializer serializer, Session session)
        {
            
            Model = model;
            _serializer = serializer;
            _session = session;
            _currentSide = Side.Client;
        }


        public IGeneratorId MessageIdGenerator { get; set; } = new TimeGeneratorId();
        protected OperationRuntimeModel Model { get; }

        public IRequestPlaceHolder<TRequest> MessagePlaceHolder { get; set; }

        bool IResponseReceiver<TResponse>.Receive(TResponse result)
        {
            if (result.StateCode != (uint)BuiltInOperationState.Handle && _responseQueue.TryRemove(new OperationId(result.Id,result.OperationCode), out var task))
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
            var op = new TRequest
            {
                OperationCode = description.Code,
                OperationData = description.UseAsyncSerialize
                    ? await _serializer.SerializeAsync(operation)
                    : _serializer.Serialize(operation),
                Id = MessageIdGenerator.Generate()
            };
            
            MessagePlaceHolder?.Fill(ref op, operation);

            var rawResult = _serializer.Serialize(op);
            await SendRawOperation(receivers, forAll, rawResult);
           
            try
            {
                if (description.WaitResponse)
                {
                    using (var composite = CancellationTokenSource.CreateLinkedTokenSource(token, GlobalToken))
                    {
                        var task = new Task<OperationResult<TResult>>(state =>
                        {
                            var s = (State) state;
                            var message = s.Result;
                            _states.Put(s);
                            if (typeof(TResult) == typeof(Empty)) return new OperationResult<TResult>(default, message.StateCode);

                            return new OperationResult<TResult>(_serializer.Deserialize<TResult>(message.OperationData.To()), message.StateCode);
                        }, _states.Rent(), composite.Token, TaskCreationOptions.PreferFairness);

                        _responseQueue.TryAdd(new OperationId(op.Id, op.OperationCode), task);
                        return await task;
                    }
                }
                return new OperationResult<TResult>(default, (uint)BuiltInOperationState.Nowaiting);
            }
            catch (OperationCanceledException)
            {
                await SendCancel(receivers, forAll, op);
                throw;
            }
            
        }

        private async Task SendCancel(IReadOnlyList<Session> receivers, bool forAll, TRequest request)
        {
            if (_responseQueue.TryRemove(new OperationId(request.Id, request.OperationCode), out var canceledTask))
            {
                _states.Put((State) canceledTask.AsyncState);
                await SendRawOperation(receivers, forAll,
                    _serializer.Serialize(new TRequest
                    {
                        Id = MessageIdGenerator.Generate(),
                        OperationCode = request.OperationCode, 
                        StateCode = (uint)BuiltInOperationState.Cancel
                    }));
            }
        }

        private async Task SendRawOperation(IReadOnlyList<Session> receivers, bool forAll, byte[] request)
        {
            switch (_currentSide)
            {
                case Side.Server when forAll:
                    await _sessions.SendToAllAsync(request);
                    break;
                
                case Side.Server:
                {
                    for (var i = 0; i < receivers.Count; i++)
                    {
                        await receivers[i].SendMessageAsync(request.To());
                    }
                    break;
                }
                case Side.Client:
                    await _session.SendMessageAsync(request.To());
                    break;
            }
        }

        private class State
        {
            public TResponse Result;
        }
       
    }
}