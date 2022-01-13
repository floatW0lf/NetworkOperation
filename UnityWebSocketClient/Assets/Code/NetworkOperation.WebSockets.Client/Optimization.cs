using System;
using Microsoft.Extensions.ObjectPool;
using WebGL.WebSockets;

namespace NetworkOperation.WebSockets.Client
{
    internal class BufferLifeTimeWrapper : IDisposable
    {
        public BufferLifeTime LifeTime { get; private set; }
        private ObjectPool<BufferLifeTimeWrapper> _pool;
        public BufferLifeTimeWrapper(ObjectPool<BufferLifeTimeWrapper> pool)
        {
            _pool = pool;
        }
        public BufferLifeTimeWrapper Setup(BufferLifeTime lifeTime)
        {
            LifeTime = lifeTime;
            return this;
        }

        public void Dispose()
        {
            LifeTime.Dispose();
            _pool.Return(this);
        }
    }
    internal class LifeTimeObjectPolicy: PooledObjectPolicy<BufferLifeTimeWrapper>, IHavePool<BufferLifeTimeWrapper>
    {
        public ObjectPool<BufferLifeTimeWrapper> Pool { get; set; }
        public override BufferLifeTimeWrapper Create()
        {
            return new BufferLifeTimeWrapper(Pool);
        }
        public override bool Return(BufferLifeTimeWrapper obj)
        {
            return true;
        }
    }

    internal interface IHavePool<T> where T : class
    {
        ObjectPool<T> Pool { get; set; }
    }

    internal class LifeTimePoolProvider : ObjectPoolProvider
    {
        public override ObjectPool<T> Create<T>(IPooledObjectPolicy<T> policy)
        {
            var pool = new DefaultObjectPool<T>(policy);
            if (policy is IHavePool<T> have)
            {
                have.Pool = pool;
            }
            return pool;
        }
    }
}