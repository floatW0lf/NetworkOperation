using System;
using System.Collections.Concurrent;

namespace NetworkOperation
{
    public class Pool<T>
    {
        private readonly Func<T> _objectFactory;
        private readonly int _expandCount;

        private ConcurrentBag<T> _bag;

        public Pool(Func<T> objectFactory, int expandCount, int startCount)
        {
            _objectFactory = objectFactory;
            _expandCount = expandCount;
            var startCollection = new T[startCount];
            for (int i = 0; i < startCount; i++)
            {
                startCollection[i] = objectFactory();
            }
            _bag = new ConcurrentBag<T>(startCollection);
        }

        public T Rent()
        {
            T item;
            while (!_bag.TryTake(out item))
            {
                for (int i = 0; i < _expandCount; i++)
                {
                    _bag.Add(_objectFactory());
                }
            }
            return item;
        }

        public void Put(T item)
        {
            _bag.Add(item);
        }

    }
}