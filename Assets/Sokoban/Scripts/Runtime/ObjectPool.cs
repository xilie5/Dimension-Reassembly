using System;
using System.Collections.Generic;
using UnityEngine;

namespace CompoundBox
{
    public sealed class ObjectPool<T> where T : Component
    {
        private readonly Func<T> create;
        private readonly Action<T> onGet;
        private readonly Action<T> onRelease;
        private readonly Stack<T> available = new Stack<T>();

        public ObjectPool(Func<T> create, Action<T> onGet = null, Action<T> onRelease = null)
        {
            this.create = create ?? throw new ArgumentNullException(nameof(create));
            this.onGet = onGet;
            this.onRelease = onRelease;
        }

        public int AvailableCount => available.Count;

        public T Get()
        {
            var item = available.Count > 0 ? available.Pop() : create();
            onGet?.Invoke(item);
            return item;
        }

        public void Release(T item)
        {
            if (item == null)
            {
                return;
            }

            onRelease?.Invoke(item);
            available.Push(item);
        }

        public void Prewarm(int count)
        {
            for (var i = 0; i < count; i++)
            {
                var item = create();
                onRelease?.Invoke(item);
                available.Push(item);
            }
        }
    }
}
