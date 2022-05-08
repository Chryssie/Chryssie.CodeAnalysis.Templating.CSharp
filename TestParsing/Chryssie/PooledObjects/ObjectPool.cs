// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace Chryssie.PooledObjects;

internal abstract class ObjectPool<T> where T : class
{
    private readonly T?[] _instances;
    private T? _firstInstance;

    public ObjectPool()
        : this(Environment.ProcessorCount * 2) { }
    public ObjectPool(int maximumRetained)
        => _instances = maximumRetained == 1 ? Array.Empty<T>() : new T?[maximumRetained - 1];

    public ObjectLease Get(out T instance)
        => new(this, instance = TryGetInstance() ?? CreateInstance() ?? throw new InvalidOperationException());

    protected T? TryGetInstance()
    {
        var instance = _firstInstance;
        if (instance is not null && Interlocked.CompareExchange(ref _firstInstance, comparand: instance, value: null) == instance)
            return instance;

        var instances = _instances;
        for (var i = 0; i < instances.Length; i++)
        {
            instance = instances[i];
            if (instance is not null && Interlocked.CompareExchange(ref instances[i], comparand: instance, value: null) == instance)
                return instance;
        }

        return null;
    }

    protected T? TryGetInstance(Predicate<T> predicate)
    {
        var instance = _firstInstance;
        if (instance is not null && predicate(instance) && Interlocked.CompareExchange(ref _firstInstance, comparand: instance, value: null) == instance)
            return instance;

        var instances = _instances;
        for (var i = 0; i < instances.Length; i++)
        {
            instance = instances[i];
            if (instance is not null && predicate(instance) && Interlocked.CompareExchange(ref instances[i], comparand: instance, value: null) == instance)
                return instance;
        }

        return null;
    }

    protected abstract T CreateInstance();

    protected ObjectLease CreateLease(T instance) => new(this, instance);

    protected abstract bool BeginFree(T obj);

    public struct ObjectLease : IDisposable
    {
        private readonly T Instance;

        private ObjectPool<T>? _pool;

        internal ObjectLease(ObjectPool<T> pool, T Instance)
        {
            _pool = pool;
            this.Instance = Instance;
        }

        public bool Free()
        {
            var pool = _pool;
            if (pool is null)
                return false;

            var instance = Instance;

            if (!pool.BeginFree(instance))
                return false;

            if (pool._firstInstance is not null || Interlocked.CompareExchange(ref pool._firstInstance, comparand: null, value: instance) is not null)
            {
                var instances = pool._instances;

                for (var i = 0; i < instances.Length && Interlocked.CompareExchange(ref instances[i], comparand: null, value: instance) is not null; i++) ;
            }

            _pool = null;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IDisposable.Dispose() => Free();
    }
}
