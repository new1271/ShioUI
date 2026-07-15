using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using RiceTea.Core.Buffers;

namespace ShioUI.Layout;

[StructLayout(LayoutKind.Auto)]
public ref partial struct VirtualLayoutContext : ILayoutContext, IDisposable
{
    private readonly PooledList<LayoutNodeBase> _walkedNonCachedNodeList;

    private LayoutContext _context;
    private Data _data;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal VirtualLayoutContext(scoped ref Builder builder)
    {
        PooledList<LayoutNodeBase> walkedNonCachedNodeList = new PooledList<LayoutNodeBase>(capacity: 0);
        _walkedNonCachedNodeList = walkedNonCachedNodeList;
        _data = builder._data;
        _context = new LayoutContext(builder._arguments, builder._data.SharedData, walkedNonCachedNodeList);
        builder._arguments = default;
        builder._data = default;
    }

    public readonly Size PageSize
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _context.PageSize;
    }

    public readonly ulong Timestamp
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _context.Timestamp;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Builder CreateVirtualContextBuilder() => _context.CreateVirtualContextBuilder();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly LayoutContext.ChildrenEnumerator GetChildrenEnumerator(UIElement element)
        => _context.GetChildrenEnumerator(element);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly int GetComputedValue(LayoutNode node)
        => _context.GetComputedValue(node);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly float GetComputedValue(FractionalLayoutNode node)
        => _context.GetComputedValue(node);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly int GetComputedValue(UIElement element, LayoutProperty property)
        => _context.GetComputedValue(element, property);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly int?[] GetComputedValues(UIElement element)
        => _context.GetComputedValues(element);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly LayoutNode? GetLayoutNodeOrNull(UIElement element, LayoutProperty property)
        => _context.GetLayoutNodeOrNull(element, property);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool TryGetParentElement(UIElement element, [NotNullWhen(true)] out UIElement? parent)
        => _context.TryGetParentElement(element, out parent);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void ClearTemporaryCacheForNodes()
    {
        PooledList<LayoutNodeBase> walkedNonCachedNodeList = _walkedNonCachedNodeList;
        foreach (LayoutNodeBase node in walkedNonCachedNodeList)
            node.ClearCache();
        walkedNonCachedNodeList.Clear();
    }

    public void Dispose()
    {
        _context = default;
        _walkedNonCachedNodeList.Dispose();
        _data.Dispose();
    }
}
