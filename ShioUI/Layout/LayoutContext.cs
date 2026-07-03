using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using RiceTea.Core.Buffers;
using RiceTea.Core.Extensions;
using RiceTea.Core.Helpers;
using RiceTea.Core.Structures;

using ShioUI.Internals;
using ShioUI.Layout.Internals;

namespace ShioUI.Layout;

[StructLayout(LayoutKind.Auto)]
public readonly unsafe ref partial struct LayoutContext : ILayoutContext
{
    private readonly Dictionary<LayoutNodeBase, int>? _walkedNodes;
    private readonly PooledList<LayoutNodeBase>? _walkedNonCachedNodeList;

    internal readonly Arguments _arguments;
    internal readonly VirtualLayoutContext.SharedData _virtualData;

    public ulong Timestamp
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _arguments.Timestamp;
    }

    public readonly Size PageSize
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _arguments.PageSize;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public LayoutContext(scoped in Arguments arguments)
    {
        _arguments = arguments;
        if (ShioSettings.UseDebugMode)
            _walkedNodes = new Dictionary<LayoutNodeBase, int>(LayoutNodeBaseEqualityComparer.Instance);
        else
            _walkedNodes = null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal LayoutContext(scoped in Arguments argument, scoped in VirtualLayoutContext.SharedData virtualData,
        PooledList<LayoutNodeBase> walkedNonCachedNodeList) : this(in argument)
    {
        _virtualData = virtualData;
        _walkedNonCachedNodeList = walkedNonCachedNodeList;
    }

    public readonly VirtualLayoutContext.Builder CreateVirtualContextBuilder()
        => new VirtualLayoutContext.Builder(in this);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ChildrenEnumerator GetChildrenEnumerator(UIElement element)
    {
        if (!_arguments.ChildrenDict.TryGetValue(element, out ArraySegment<UIElement> segment))
            return default;
        UIElement[]? array = segment.Array;
        int offset = segment.Offset;
        int count = segment.Count;
        if (array is null || offset < 0 || count <= 0)
            return default;
        return new ChildrenEnumerator(array, offset, count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool TryGetParentElement(UIElement element, [NotNullWhen(true)] out UIElement? parent)
        => _arguments.ParentDict.TryGetValue(element, out parent);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly LayoutNode? GetLayoutNodeOrNull(UIElement element, LayoutProperty property)
    {
        if (property >= LayoutProperty._Last)
            return ArgumentOutOfRangeException.Throw<LayoutNode>(nameof(property));

        if (!_arguments.ElementDict.TryGetValue(element, out ArraySegment<LayoutNode?> segment))
            return null;

        return UnsafeHelper.AddTypedOffset(ref UnsafeHelper.GetArrayDataReference(segment.Array!), segment.Offset + (int)property);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly int?[] GetComputedValues(UIElement element)
    {
        int?[] result = ArrayHelper.CreateUninitializedArray<int?>((int)LayoutProperty._Last);
        ref int? resultRef = ref UnsafeHelper.GetArrayDataReference(result);

        if (!_arguments.ElementDict.TryGetValue(element, out ArraySegment<LayoutNode?> segment))
        {
            Rect bounds = element.Bounds;
            UnsafeHelper.AddTypedOffset(ref resultRef, (nuint)LayoutProperty.Left) = bounds.Left;
            UnsafeHelper.AddTypedOffset(ref resultRef, (nuint)LayoutProperty.Top) = bounds.Top;
            UnsafeHelper.AddTypedOffset(ref resultRef, (nuint)LayoutProperty.Right) = bounds.Right;
            UnsafeHelper.AddTypedOffset(ref resultRef, (nuint)LayoutProperty.Bottom) = bounds.Bottom;
            UnsafeHelper.AddTypedOffset(ref resultRef, (nuint)LayoutProperty.Width) = bounds.Width;
            UnsafeHelper.AddTypedOffset(ref resultRef, (nuint)LayoutProperty.Height) = bounds.Height;
            return result;
        }

        ref LayoutNode? nodeRef = ref UnsafeHelper.AddTypedOffset(ref UnsafeHelper.GetArrayDataReference(segment.Array!), segment.Offset);

        for (nuint property = (nuint)LayoutProperty.Left; property < (nuint)LayoutProperty._Last; property++)
        {
            LayoutNode? node = UnsafeHelper.AddTypedOffset(ref nodeRef, property);
            if (node is null)
                continue;
            UnsafeHelper.AddTypedOffset(ref resultRef, property) = GetComputedValue(node);
        }
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly int GetComputedValue(UIElement element, LayoutProperty property)
    {
        if (property >= LayoutProperty._Last)
            return ArgumentOutOfRangeException.Throw<int>(nameof(property));

        if (!_arguments.ElementDict.TryGetValue(element, out ArraySegment<LayoutNode?> segment))
        {
            return property switch
            {
                LayoutProperty.Left => element.Left,
                LayoutProperty.Top => element.Top,
                LayoutProperty.Right => element.Right,
                LayoutProperty.Bottom => element.Bottom,
                LayoutProperty.Width => element.Width,
                LayoutProperty.Height => element.Height,
                _ => ArgumentOutOfRangeException.Throw<int>(nameof(property))
            };
        }

        ref readonly LayoutNode? nodeRef = ref UnsafeHelper.AddTypedOffsetAsReadOnly(ref UnsafeHelper.GetArrayDataReference(segment.Array!), segment.Offset);
        LayoutNode? node = UnsafeHelper.AddTypedOffsetAsReadOnly(in nodeRef, (nuint)property);
        if (node is not null)
            return GetComputedValue(node);

        return property switch
        {
            LayoutProperty.Left => GetComputedValueOrZero(UnsafeHelper.AddTypedOffsetAsReadOnly(in nodeRef, (nuint)LayoutProperty.Right)) -
                GetComputedValueOrZero(UnsafeHelper.AddTypedOffsetAsReadOnly(in nodeRef, (nuint)LayoutProperty.Width)),
            LayoutProperty.Top => GetComputedValueOrZero(UnsafeHelper.AddTypedOffsetAsReadOnly(in nodeRef, (nuint)LayoutProperty.Bottom)) -
                GetComputedValueOrZero(UnsafeHelper.AddTypedOffsetAsReadOnly(in nodeRef, (nuint)LayoutProperty.Height)),
            LayoutProperty.Right => GetComputedValueOrZero(UnsafeHelper.AddTypedOffsetAsReadOnly(in nodeRef, (nuint)LayoutProperty.Left)) +
                GetComputedValueOrZero(UnsafeHelper.AddTypedOffsetAsReadOnly(in nodeRef, (nuint)LayoutProperty.Width)),
            LayoutProperty.Bottom => GetComputedValueOrZero(UnsafeHelper.AddTypedOffsetAsReadOnly(in nodeRef, (nuint)LayoutProperty.Top)) +
                GetComputedValueOrZero(UnsafeHelper.AddTypedOffsetAsReadOnly(in nodeRef, (nuint)LayoutProperty.Height)),
            LayoutProperty.Width => GetComputedValueOrZero(UnsafeHelper.AddTypedOffsetAsReadOnly(in nodeRef, (nuint)LayoutProperty.Right)) -
                GetComputedValueOrZero(UnsafeHelper.AddTypedOffsetAsReadOnly(in nodeRef, (nuint)LayoutProperty.Left)),
            LayoutProperty.Height => GetComputedValueOrZero(UnsafeHelper.AddTypedOffsetAsReadOnly(in nodeRef, (nuint)LayoutProperty.Bottom)) -
                GetComputedValueOrZero(UnsafeHelper.AddTypedOffsetAsReadOnly(in nodeRef, (nuint)LayoutProperty.Top)),
            _ => ArgumentOutOfRangeException.Throw<int>(nameof(property))
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly int GetComputedValueOrZero(LayoutNode? variable)
        => variable is null ? 0 : GetComputedValue(variable);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly int GetComputedValue(LayoutNode node)
    {
        if (node is FixedValueLayoutNode fixedValueNode)
            return fixedValueNode.Value;

        LayoutNode[]? fakeLayoutNodeKeys = _virtualData.FakeLayoutNodeKeys;
        if (fakeLayoutNodeKeys is not null)
        {
            int count = _virtualData.FakeLayoutNodeCount;
            int indexOf = Array.IndexOf(fakeLayoutNodeKeys, node, 0, count);
            if (indexOf >= 0 && indexOf < count)
                return _virtualData.FakeLayoutNodeValues[indexOf];
        }

        PooledList<LayoutNodeBase>? nodeList = _walkedNonCachedNodeList;
        if (nodeList is null)
            return GetComputedValue_SlowRoute(node);
        else
            return GetComputedValue_SlowRouteAndCheckCached(nodeList, node);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly float GetComputedValue(FractionalLayoutNode node)
    {
        if (node is FixedValueLayoutNode.Fractional fixedValueNode)
            return fixedValueNode.Value;

        FractionalLayoutNode[]? fakeFractionalLayoutNodeKeys = _virtualData.FakeFractionalLayoutNodeKeys;
        if (fakeFractionalLayoutNodeKeys is not null)
        {
            int count = _virtualData.FakeFractionalLayoutNodeCount;
            int indexOf = Array.IndexOf(fakeFractionalLayoutNodeKeys, node, 0, count);
            if (indexOf >= 0 && indexOf < count)
                return _virtualData.FakeFractionalLayoutNodeValues[indexOf];
        }

        PooledList<LayoutNodeBase>? nodeList = _walkedNonCachedNodeList;
        if (nodeList is null)
            return GetComputedValue_SlowRoute(node);
        else
            return GetComputedValue_SlowRouteAndCheckCached(nodeList, node);
    }

    private readonly int GetComputedValue_SlowRoute(LayoutNode node)
    {
        AddNodeOrThrow(node);
        try
        {
            return node.ComputeInternal(this);
        }
        finally
        {
            RemoveNode(node);
        }
    }

    private readonly float GetComputedValue_SlowRoute(FractionalLayoutNode node)
    {
        AddNodeOrThrow(node);
        try
        {
            return node.ComputeInternal(this);
        }
        finally
        {
            RemoveNode(node);
        }
    }

    private readonly int GetComputedValue_SlowRouteAndCheckCached(PooledList<LayoutNodeBase> list, LayoutNode node)
    {
        AddNodeOrThrow(node);
        try
        {
            (int result, bool cached) = node.ComputeInternalWithCached(this);
            if (!cached)
                list.Add(node);
            return result;
        }
        finally
        {
            RemoveNode(node);
        }
    }

    private readonly float GetComputedValue_SlowRouteAndCheckCached(PooledList<LayoutNodeBase> list, FractionalLayoutNode node)
    {
        AddNodeOrThrow(node);
        try
        {
            (float result, bool cached) = node.ComputeInternalWithCached(this);
            if (!cached)
                list.Add(node);
            return result;
        }
        finally
        {
            RemoveNode(node);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddNodeOrThrow(LayoutNodeBase node)
    {
        Dictionary<LayoutNodeBase, int>? walkedNodes = _walkedNodes;
        if (walkedNodes is null)
            return;
        if (walkedNodes.ContainsKey(node))
            ThrowCyclicDependencyException(walkedNodes);
        else
            walkedNodes.Add(node, walkedNodes.Count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RemoveNode(LayoutNodeBase node) => _walkedNodes?.Remove(node);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ThrowCyclicDependencyException(LayoutNodeBase node)
    {
        Dictionary<LayoutNodeBase, int>? walkedNodes = _walkedNodes;
        if (walkedNodes is null)
            ThrowWithNoArgs(node);
        else
        {
            walkedNodes.TryAdd(node, walkedNodes.Count);
            ThrowCyclicDependencyException(walkedNodes);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void ThrowWithNoArgs(LayoutNodeBase node) => throw new CyclicDependencyException([node]);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowCyclicDependencyException(Dictionary<LayoutNodeBase, int> nodes)
        => throw new CyclicDependencyException(
            nodes.OrderBy(static pair => pair.Value)
            .Select(static pair => pair.Key)
            .ToArray()
            );

    public ref struct ChildrenEnumerator : IEnumerator<UIElement>
    {
        private readonly UIElement[] _array;
        private int _offset, _count, _index;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ChildrenEnumerator(UIElement[] array, int offset, int count)
        {
            _array = array;
            _offset = offset;
            _count = count;
            _index = -1;
        }

        public readonly UIElement Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                int index = _index;
                if (index < 0 || index >= _count)
                    return InvalidOperationException.Throw<UIElement>();
                return _array.AsUnsafeRef()[_offset + index];
            }
        }

        readonly object? IEnumerator.Current => Current;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _offset = 0;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset() => _index = -1;
    }
}
