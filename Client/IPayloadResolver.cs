using System;
using NetworkOperation.Extensions;

namespace NetworkOperation.Client
{
    public interface IPayloadResolver
    {
        ArraySegment<byte> Resolve();
        ArraySegment<byte> Resolve<T>(T payload) where T : IConnectPayload;
    }
    
    public abstract class PayloadResolver : IPayloadResolver
    {
        protected readonly BaseSerializer Serializer;

        protected PayloadResolver(BaseSerializer serializer)
        {
            Serializer = serializer;
        }

        public abstract ArraySegment<byte> Resolve();
        
        public ArraySegment<byte> Resolve<T>(T payload) where T : IConnectPayload
        {
            return Serializer.Serialize(payload).To();
        }
    }

    public class PayloadResolver<T> : PayloadResolver where T : IConnectPayload
    {
        private readonly T _payloadValue;
        private readonly Func<T> _payloadFactory;

        public PayloadResolver(Func<T> payloadFactory, BaseSerializer serializer) : base(serializer)
        {
            _payloadFactory = payloadFactory;
        }
        
        public PayloadResolver(T payloadValue, BaseSerializer serializer) : base(serializer)
        {
            _payloadValue = payloadValue;
        } 

        public override ArraySegment<byte> Resolve()
        {
            return Serializer.Serialize(_payloadFactory != null ? _payloadFactory() : _payloadValue).To();
        }
    }

    public sealed class NullPayloadResolver : IPayloadResolver
    {
        public ArraySegment<byte> Resolve()
        {
            return new ArraySegment<byte>();
        }

        public ArraySegment<byte> Resolve<T>(T payload) where T : IConnectPayload
        {
            return new ArraySegment<byte>();
        }
    }
}