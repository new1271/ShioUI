using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using RiceTea.Core.Extensions;
using RiceTea.Core.Helpers;

namespace ShioUI.Caching;

partial class CacheStore<T>
{
    [StructLayout(LayoutKind.Auto)]
    public ref struct Scope : IDisposable
    {
        private Node? _node;

        public readonly int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _node!.Count; // throws NRE here if disposed
        }

        public readonly ulong Timestamp
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _node!.Timestamp; // throws NRE here if disposed
        }

        public readonly T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                Node? node = _node;
                if (index < 0 || index >= node!.Count) // throws NRE here if disposed
                    IndexOutOfRangeException.Throw();
                T[]? array = node.Array;
                DebugHelper.ThrowIf(array is null);
                return array.AsUnsafeRef()[index];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Scope(object node) => _node = (Node)node;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyTo(T[] destination)
        {
            Node? node = _node;
            T[]? array = node!.Array; // throws NRE here if disposed
            DebugHelper.ThrowIf(array is null);
            Array.Copy(array, destination, node.Count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void CopyTo(T[] destination, int startIndex)
        {
            Node? node = _node;
            T[]? array = node!.Array; // throws NRE here if disposed
            DebugHelper.ThrowIf(array is null);
            Array.Copy(array, 0, destination, startIndex, node.Count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ref readonly T GetReferenceOfFirstElement()
        {
            T[]? array = _node!.Array; // throws NRE here if disposed
            DebugHelper.ThrowIf(array is null);
            return ref UnsafeHelper.GetArrayDataReference(array);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly T[] ToArray()
        {
            Node? node = _node;
            T[]? array = node!.Array; // throws NRE here if disposed
            DebugHelper.ThrowIf(array is null);
            return ArrayHelper.CopyItemsToArrayUnsafe(ref UnsafeHelper.GetArrayDataReference(array), node.Count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Enumerator GetEnumerator()
        {
            Node? node = _node;
            T[]? array = node!.Array; // throws NRE here if disposed
            DebugHelper.ThrowIf(array is null);
            return new Enumerator(array, node.Count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            Node? node = _node;
            if (node is null)
                return;
            _node = node;
            node.Owner!.Dereference(node);
        }

        [StructLayout(LayoutKind.Auto)]
        public ref struct Enumerator : IEnumerator<T>
        {
            private T[]? _array;
            private int _count, _index;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(T[] array, int count)
            {
                _array = array;
                _count = count;
                _index = -1;
            }

            public readonly T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    int index = _index;
                    if (index < 0 || index >= _count)
                        return InvalidOperationException.Throw<T>();
                    return _array!.AsUnsafeRef()[index];
                }
            }

            readonly object? IEnumerator.Current => Current;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
                _array = null;
                _index = -1;
                _count = 0;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                int index = _index + 1;
                int count = _count;
                if (index < count)
                {
                    _index = index;
                    return index >= 0;
                }
                return false;
            }

            public void Reset() => _index = -1;
        }
    }

}
