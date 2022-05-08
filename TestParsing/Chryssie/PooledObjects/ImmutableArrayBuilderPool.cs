// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Chryssie.PooledObjects;

internal sealed class ImmutableArrayBuilderPool<T> : ObjectPool<ImmutableArray<T>.Builder>
{
    private static ImmutableArrayBuilderPool<T>? s_defaultPool;
    public static ImmutableArrayBuilderPool<T> Default
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

    public ImmutableArrayBuilderPool()
        : base() { }
    public ImmutableArrayBuilderPool(int maximumRetained)
        : base(maximumRetained) { }

    protected override bool BeginFree(ImmutableArray<T>.Builder obj)
    {
        if (obj.Capacity >= 128)
            return false;

        if (obj.Count != 0)
            obj.Clear();

        return true;
    }

    protected override ImmutableArray<T>.Builder CreateInstance()
        => ImmutableArray.CreateBuilder<T>(8);
}

internal sealed class StackPool<T> : ObjectPool<Stack<T>>
{
    private static StackPool<T>? s_defaultPool;
    public static StackPool<T> Default
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

    public StackPool()
        : base() { }
    public StackPool(int maximumRetained)
        : base(maximumRetained) { }

    protected override bool BeginFree(Stack<T> obj)
    {
        if (obj.Capacity >= 128)
            return false;

        if (obj.Count != 0)
            obj.Clear();

        return true;
    }

    protected override Stack<T> CreateInstance()
        => new(8);
}

internal abstract class RingList<T> : IEnumerable<T>, IReadOnlyCollection<T>
{
    private const int DefaultCapacity = 4;

    private protected T[] _array;
    private protected int _start, _count, _version;

    protected RingList()
    {
        _array = Array.Empty<T>();
        _start = 0;
        _count = 0;
    }

    public int Count => _count;

    protected RingList(int capacity)
    {
        _array = capacity switch
        {
            < 0 => throw new ArgumentOutOfRangeException(paramName: nameof(capacity), capacity, message: $"Value must not be negative."),
            0 => Array.Empty<T>(),
            > 0 => new T[capacity],
        };
        _start = 0;
        _count = 0;
    }

    protected RingList(IEnumerable<T> collection)
    {
        if (collection is null)
            throw new ArgumentNullException(nameof(collection));

        _array = collection.ToArray();
        _start = 0;
        _count = _array.Length;
    }

    protected void Add(T item)
    {
        if ((uint)_count < (uint)_array.Length)
            AddWithoutResize(item);
        else
            AddWithResize(item);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void AddWithResize(T item)
    {
        Debug.Assert(_count == _array.Length);
        Grow(_count + 1);
        AddWithoutResize(item);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddWithoutResize(T item)
    {
        var array = _array;
        var count = _count;

        var index = _start + count;
        if (index >= array.Length)
            index -= array.Length;

        _array[index] = item;
        _count = count + 1;
        _version++;
    }

    // Gets and sets the capacity of this list.  The capacity is the size of
    // the internal array used to hold items.  When set, the internal
    // array of the list is reallocated to the given capacity.
    //
    public int Capacity
    {
        get => _array.Length;
        set
        {
            if (value < _count)
                throw new ArgumentOutOfRangeException(paramName: nameof(value), actualValue: value, message: $"Capacity cannot be smaller than count.");

            if (value != _array.Length)
                Grow(value);
        }
    }

    public void Clear()
    {
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            var array = _array;
            var start = _start;
            var count = _count;

            if (count > 0)
            {
                var leftCount = count - (array.Length - start);
                if (leftCount <= 0)
                {
                    // No wrapping is present, so we can just do one clear.
                    Array.Clear(array, start, count);
                }
                else
                {
                    // The items are wrapped, so we need to do two clears. First from start to the end of the array and then from the start of the array until the rest of the remainingCount.
                    var rightCount = count - leftCount;
                    Array.Clear(array, 0, leftCount);
                    Array.Clear(array, start, rightCount);
                }
            }
        }

        _count = 0;
        _version++;
    }

    #region Peek

    protected ref T PeekFirst()
    {
        if (_count == 0)
            throw new InvalidOperationException();

        return ref _array[_start];
    }

    protected ref T PeekLast()
    {
        switch (_count)
        {
            case 0: throw new InvalidOperationException();
            case 1: return ref _array[_start];
            case var count:
                var array = _array;
                var index = _start + count - 1;
                if (index >= array.Length)
                    index -= array.Length;

                return ref _array[index];
        }
    }

    protected bool TryPeekFirst([MaybeNullWhen(false)] out T result)
    {
        if (_count == 0)
        {
            result = default;
            return false;
        }

        result = _array[_start];
        return true;
    }

    protected bool TryPeekLast([MaybeNullWhen(false)] out T result)
    {
        switch (_count)
        {
            case 0:
                result = default;
                return false;
            case 1:
                result = _array[_start];
                return true;
            case var count:
                var array = _array;
                var index = _start + count - 1;
                if (index >= array.Length)
                    index -= array.Length;

                result = _array[index];
                return true;
        }
    }

    #endregion

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Grow(int capacity)
    {
        const int MaxArrayLength = 0X7FFFFFC7;

        var array = _array;
        var start = _start;
        var count = _count;

        Debug.Assert(array.Length < capacity);

        var newCapacity = array.Length == 0 ? RingList<T>.DefaultCapacity : 2 * (uint)array.Length;

        if (newCapacity > MaxArrayLength)
            newCapacity = MaxArrayLength;

        if (newCapacity < (uint)capacity)
            newCapacity = (uint)capacity;

        var newArray = new T[newCapacity];

        if (count > 0)
        {
            var leftCount = count - (array.Length - start);
            if (leftCount <= 0)
            {
                // No wrapping is present, so we can just do one copy.
                Array.Copy(
                    sourceArray: array,
                    sourceIndex: start,
                    destinationArray: newArray,
                    destinationIndex: start,
                    length: count
                );
            }
            else
            {
                // The items are wrapped, so we need to do two copies.
                //   1: From the start of the array until the rest of the remainingCount.
                //   2: From start to the end of the array.

                var rightCount = count - leftCount;

                Array.Copy(
                    sourceArray: array,
                    sourceIndex: 0,
                    destinationArray: newArray,
                    destinationIndex: start + rightCount,
                    length: leftCount
                );
                Array.Copy(
                    sourceArray: array,
                    sourceIndex: start,
                    destinationArray: newArray,
                    destinationIndex: start,
                    length: rightCount
                );
            }
        }

        _array = newArray;
    }

    protected abstract IEnumerator<T> GetEnumeratorObject();

    /// <inheritdoc />
    public IEnumerator<T> GetEnumerator() => GetEnumeratorObject();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

internal sealed class RingStack<T> : RingList<T>
{
    public RingStack()
        : base() { }

    public RingStack(int capacity)
        : base(capacity) { }

    public RingStack(IEnumerable<T> collection)
        : base(collection) { }

    public void Push(T item) => this.Add(item);

    public T Peek() => TryPeek(out var result) ? result : throw new InvalidOperationException();
    public bool TryPeek([MaybeNullWhen(false)] out T result)
    {
        switch (_count)
        {
            case 0:
                result = default;
                return false;
            case 1:
                result = _array[_start];
                return true;
            case var count:
                var array = _array;
                var index = _start + count - 1;
                if (index >= array.Length)
                    index -= array.Length;

                result = _array[index];
                return true;
        }
    }

    public T Pop() => TryPop(out var result) ? result : throw new InvalidOperationException();
    public bool TryPop([MaybeNullWhen(false)] out T result)
    {
        T[] array;
        int index;

        var count = _count;

        switch (_count)
        {
            case 0:
                result = default;
                return false;
            case 1:
                array = _array;
                index = _start;
                break;
            default:
                array = _array;
                index = _start + count - 1;
                if (index >= array.Length)
                    index -= array.Length;
                break;
        }

        _count = count - 1;

        result = array[index];
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            array[index] = default!;

        _version++;


        return true;
    }
}

internal sealed class RingQueue<T> : RingList<T>
{
    public RingQueue()
        : base() { }

    public RingQueue(int capacity)
        : base(capacity) { }

    public RingQueue(IEnumerable<T> collection)
        : base(collection) { }

    public void Enqueue(T item) => Add(item);

    public T Peek() => TryPeek(out var result) ? result : throw new InvalidOperationException();
    public bool TryPeek([MaybeNullWhen(false)] out T result)
    {
        if (_count == 0)
        {
            result = default;
            return false;
        }

        result = _array[_start];
        return true;
    }

    public T Pop() => TryPop(out var result) ? result : throw new InvalidOperationException();
    public bool TryPop([MaybeNullWhen(false)] out T result)
    {
        var count = _count;
        if (count == 0)
        {
            result = default;
            return false;
        }

        var array = _array;
        var start = _start;

        result = array[start];
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            array[start] = default!;

        var newStart = start + 1;
        if (newStart >= array.Length)
            newStart -= array.Length;

        _start = newStart;
        _count = count - 1;

        _version++;

        return true;
    }

    protected override IEnumerator<T> GetEnumeratorObject()
    {
        throw new NotImplementedException();
    }

    public struct Enumerator : IEnumerator<T>
    {
        const int ClearAndFinish = -3;
        const int Finish = -2;
        const int Start = -1;

        private readonly RingQueue<T> _queue;
        private readonly int _version;
        private uint _state, _target, _nextTarget;
        private T? _current;

        internal Enumerator(RingQueue<T> queue)
        {
            _queue = queue;
            _version = queue._version;
            _state = -1;
            _state = 0;
            _target = 0;
            _nextTarget = Start;
            _current = default;
        }

        public readonly T Current => _current;
        readonly object IEnumerator.Current => Current;

        public readonly void Dispose() { }

        public bool MoveNext()
        {
            var queue = _queue;

            if (_version == queue._version && ((uint)_index < (uint)_state))
            {
                _current = queue._array[_index];
                _index++;
                return true;
            }

            return MoveNextRare();
        }

        private bool MoveNextRare()
        {
            var queue = _queue;
            if (_version != queue._version)
                throw new InvalidOperationException();

            switch (_nextState)
            {
                case ClearAndFinish:
                    _current = default;
                    _nextState = -2;
                    goto case -2;
                case Finish: return false;
                case -1:
                    var count = queue._count;

                    if (count == 0)
                    {
                        _nextState = -2;
                        return false;
                    }

                    var array = queue._array;
                    var start = queue._start;

                    _nextState = count - (array.Length - start);
                    if (_nextState <= 0)
                    {
                        _nextState = 0;
                        _state = start + count;
                    }
                    else
                    {
                        _state = start + count - _nextState;
                    }

                    _current = array[start];
                    _index = start + 1;
                    break;
                case var nextEnd:
                    _state = 
                    _index = 1;
                    break;
            }
        }

        public void Reset()
        {
            if (_version != _queue._version)
                throw new InvalidOperationException();

            _index = 0;
            _state = 0;
            _nextState = 0;
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                _current = default;
        }
    }
}
