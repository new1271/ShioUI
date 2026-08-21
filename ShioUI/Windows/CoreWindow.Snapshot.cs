using System.Runtime.CompilerServices;

using RiceTea.Core.Buffers;
using RiceTea.Core.Extensions;

using ShioUI.Caching;

namespace ShioUI.Windows;

partial class CoreWindow
{
    private static readonly ArrayPool<UIElement?> _elementArrayPool = ArrayPool<UIElement?>.Shared;
    private static readonly ArrayPool<IWindowMessageFilter> _windowMessageFilterPool = ArrayPool<IWindowMessageFilter>.Shared;

    private readonly CacheStore<UIElement?> _activeElementsCacheStore, _elementsCacheStore;
    private readonly CacheStore<IWindowMessageFilter> _windowMessageFilterStore;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected CacheStore<UIElement?>.Scope EnterActiveElementsCacheScope() => _activeElementsCacheStore.GetLastSnapshot();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected CacheStore<UIElement?>.Scope EnterElementsCacheScope() => _elementsCacheStore.GetLastSnapshot();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected CacheStore<IWindowMessageFilter>.Scope EnterWindowMessageFilterCacheScope() => _windowMessageFilterStore.GetLastSnapshot();

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static CacheStore<UIElement?>.Body CreateSnapshotForActiveElements(object owner)
    {
        CoreWindow _this = (CoreWindow)owner;
        ArrayPool<UIElement?> pool = _elementArrayPool;
        (UIElement?[] elements, int count) = pool.EnterRentScopeAndCapture(_this.EnumerateActiveElements());
        return new(elements, count);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static CacheStore<UIElement?>.Body CreateSnapshotForElements(object owner)
    {
        CoreWindow _this = (CoreWindow)owner;
        ArrayPool<UIElement?> pool = _elementArrayPool;
        (UIElement?[] elements, int count) = pool.EnterRentScopeAndCapture(_this.EnumerateElements());
        return new(elements, count);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static CacheStore<IWindowMessageFilter>.Body CreateSnapshotForWindowMessageFilter(object owner)
    {
        CoreWindow _this = (CoreWindow)owner;
        ArrayPool<IWindowMessageFilter> pool = _windowMessageFilterPool;
        (IWindowMessageFilter[] elements, int count) = pool.EnterRentScopeAndCapture(_this._windowMessageFilters);
        return new(elements, count);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void DropSnapshot(object owner, in CacheStore<UIElement?>.Body body)
    {
        ArrayPool<UIElement?> pool = _elementArrayPool;
        pool.Return(body.Array);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void DropSnapshot(object owner, in CacheStore<IWindowMessageFilter>.Body body)
    {
        ArrayPool<IWindowMessageFilter> pool = _windowMessageFilterPool;
        pool.Return(body.Array);
    }
}
