// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace System.Collections.Immutable;

internal static class ImmutableExtensions
{
    public static ImmutableArray<T> ToImmutableAndClear<T>(this ImmutableArray<T>.Builder builder)
    {
        if (builder.Count == 0)
            return ImmutableArray<T>.Empty;

        if (builder.Count == builder.Capacity)
            return builder.MoveToImmutable();

        var result = builder.ToImmutable();
        builder.Clear();
        return result;
    }

    public static void Push<T>(this ImmutableArray<T>.Builder builder, T item) => builder.Add(item);
    public static void Enqueue<T>(this ImmutableArray<T>.Builder builder, T item) => builder.Add(item);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Pop<T>(this ImmutableArray<T>.Builder builder)
    {
        builder.RemoveAt(^1, out var item);
        return item;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ImmutableArray<T> RemoveAt<T>(this ImmutableArray<T> immutableArray, Index index, out T item)
        => immutableArray.RemoveAt(index.GetOffset(immutableArray.Length), out item);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RemoveAt<T>(this ImmutableArray<T>.Builder builder, Index index, out T item)
        => builder.RemoveAt(index.GetOffset(builder.Count), out item);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ImmutableArray<T> RemoveAt<T>(this ImmutableArray<T> immutableArray, int index, out T item)
    {
        item = immutableArray[index];
        return immutableArray.RemoveAt(index);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RemoveAt<T>(this ImmutableArray<T>.Builder builder, int index, out T item)
    {
        item = builder[index];
        builder.RemoveAt(index);
    }
}
