// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace Roslyn.Utilities
{
    /// <summary>
    /// this is RAII object to automatically release pooled object when its owning pool
    /// </summary>
    internal struct PooledObject<T> : IDisposable where T : class
    {
        private readonly Action<ObjectPool<T>, T, PooledObjectToken> _releaser;
        private readonly ObjectPool<T> _pool;
        private readonly PooledObjectToken _token;
        private T _pooledObject;

        public PooledObject(ObjectPool<T> pool, Func<ObjectPool<T>, (T, PooledObjectToken)> allocator, Action<ObjectPool<T>, T, PooledObjectToken> releaser) : this()
        {
            _pool = pool;
            (_pooledObject, _token) = allocator(pool);
            _releaser = releaser;
        }

        public T Object
        {
            get
            {
#if DEBUG
                if (_token != PooledObjectToken.None && _pooledObject is IPoolableObject poolable)
                {
                    Contract.ThrowIfFalse(_token == poolable.Token, "A pooled object was used after release.");
                }
#endif

                return _pooledObject;
            }
        }

        public void Dispose()
        {
            if (_pooledObject != null)
            {
                _releaser(_pool, _pooledObject, _token);
                _pooledObject = null;
            }
        }

        #region factory
        public static PooledObject<StringBuilder> Create(ObjectPool<StringBuilder> pool)
        {
            return new PooledObject<StringBuilder>(pool, Allocator, Releaser);
        }

        public static PooledObject<Stack<TItem>> Create<TItem>(ObjectPool<Stack<TItem>> pool)
        {
            return new PooledObject<Stack<TItem>>(pool, Allocator, Releaser);
        }

        public static PooledObject<Queue<TItem>> Create<TItem>(ObjectPool<Queue<TItem>> pool)
        {
            return new PooledObject<Queue<TItem>>(pool, Allocator, Releaser);
        }

        public static PooledObject<HashSet<TItem>> Create<TItem>(ObjectPool<HashSet<TItem>> pool)
        {
            return new PooledObject<HashSet<TItem>>(pool, Allocator, Releaser);
        }

        public static PooledObject<Dictionary<TKey, TValue>> Create<TKey, TValue>(ObjectPool<Dictionary<TKey, TValue>> pool)
        {
            return new PooledObject<Dictionary<TKey, TValue>>(pool, Allocator, Releaser);
        }

        public static PooledObject<List<TItem>> Create<TItem>(ObjectPool<List<TItem>> pool)
        {
            return new PooledObject<List<TItem>>(pool, Allocator, Releaser);
        }
        #endregion

        #region allocators and releasers
        private static (StringBuilder, PooledObjectToken) Allocator(ObjectPool<StringBuilder> pool)
        {
            return (pool.AllocateAndClear(), PooledObjectToken.None);
        }

        private static void Releaser(ObjectPool<StringBuilder> pool, StringBuilder sb, PooledObjectToken token)
        {
            pool.ClearAndFree(sb);
        }

        private static (Stack<TItem>, PooledObjectToken) Allocator<TItem>(ObjectPool<Stack<TItem>> pool)
        {
            return (pool.AllocateAndClear(), PooledObjectToken.None);
        }

        private static void Releaser<TItem>(ObjectPool<Stack<TItem>> pool, Stack<TItem> obj, PooledObjectToken token)
        {
            pool.ClearAndFree(obj);
        }

        private static (Queue<TItem>, PooledObjectToken) Allocator<TItem>(ObjectPool<Queue<TItem>> pool)
        {
            return (pool.AllocateAndClear(), PooledObjectToken.None);
        }

        private static void Releaser<TItem>(ObjectPool<Queue<TItem>> pool, Queue<TItem> obj, PooledObjectToken token)
        {
            pool.ClearAndFree(obj);
        }

        private static (HashSet<TItem>, PooledObjectToken) Allocator<TItem>(ObjectPool<HashSet<TItem>> pool)
        {
            return (pool.AllocateAndClear(), PooledObjectToken.None);
        }

        private static void Releaser<TItem>(ObjectPool<HashSet<TItem>> pool, HashSet<TItem> obj, PooledObjectToken token)
        {
            pool.ClearAndFree(obj);
        }

        private static (Dictionary<TKey, TValue>, PooledObjectToken) Allocator<TKey, TValue>(ObjectPool<Dictionary<TKey, TValue>> pool)
        {
            return (pool.AllocateAndClear(), PooledObjectToken.None);
        }

        private static void Releaser<TKey, TValue>(ObjectPool<Dictionary<TKey, TValue>> pool, Dictionary<TKey, TValue> obj, PooledObjectToken token)
        {
            pool.ClearAndFree(obj);
        }

        private static (List<TItem>, PooledObjectToken) Allocator<TItem>(ObjectPool<List<TItem>> pool)
        {
            return (pool.AllocateAndClear(), PooledObjectToken.None);
        }

        private static void Releaser<TItem>(ObjectPool<List<TItem>> pool, List<TItem> obj, PooledObjectToken token)
        {
            pool.ClearAndFree(obj);
        }
        #endregion
    }
}
