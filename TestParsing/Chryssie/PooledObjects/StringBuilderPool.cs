// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text;

namespace Chryssie.PooledObjects;

internal sealed class StringBuilderPool : ObjectPool<StringBuilder>
{
    private static StringBuilderPool? s_defaultPool;
    public static StringBuilderPool Default
    {
        get
        {
            var defaultPool = s_defaultPool;
            if (defaultPool is null)
            {
                defaultPool = new();
                defaultPool = Interlocked.CompareExchange(ref s_defaultPool, comparand: null, value: defaultPool) ?? defaultPool;
            }
            return defaultPool;
        }
    }

    public StringBuilderPool()
        : base() { }
    public StringBuilderPool(int maximumRetained)
        : base(maximumRetained) { }

    protected override bool BeginFree(StringBuilder obj)
    {
        if (obj.Capacity >= 1024)
            return false;

        if (obj.Length != 0)
            obj.Clear();

        return true;
    }

    protected override StringBuilder CreateInstance() => new();
}
