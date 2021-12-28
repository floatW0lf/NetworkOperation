using System;
using NetworkOperation.Core;

namespace NetworkOperation.Client
{
    public interface IPayloadResolver
    {
        ArraySegment<byte> Resolve();
        ArraySegment<byte> Resolve<T>(T payload) where T : IConnectPayload;
    }
    
    public class PayloadResolver : IPayloadResolver
    {
        protected readonly BaseSerializer Serializer;

        public PayloadResolver(BaseSerializer serializer)
        {
            Serializer = serializer;
        }

        public virtual ArraySegment<byte> Resolve()
        {
            throw new NotImplementedException();
        }
        
        public ArraySegment<byte> Resolve<T>(T payload) where T : IConnectPayload
        {
            return Serializer.Serialize(payload,null).To();
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
            return Serializer.Serialize(_payloadFactory != null ? _payloadFactory() : _payloadValue,null).To();
        }
    }

    public sealed class NullPayloadResolver : IPayloadResolver
    {
        private static readonly byte[] NullPayload = { 0 };
        public ArraySegment<byte> Resolve()
        {
            return NullPayload.To();
        }

        public ArraySegment<byte> Resolve<T>(T payload) where T : IConnectPayload
        {
            return NullPayload.To();
        }
    }
}