using System;
using System.Runtime.CompilerServices;
using System.Threading;

using RiceTea.Core;
using RiceTea.Core.Helpers;

namespace ShioUI.Graphics;

public abstract partial class RenderElement : ICheckableDisposable
{
    private static int _identifierGenerator = 0;

    private readonly Lock _syncLock;
    private readonly int _identifier;
    private readonly bool _enablePartialRendering;

    private nuint _needRefresh, _shouldUpdateWhenUnfreeze, _freezeCount, _disposed;

    protected RenderElement()
    {
        _syncLock = new Lock();
        _identifier = Atomics.Increment(ref _identifierGenerator);
        _needRefresh = UnsafeHelper.GetMaxValue<nuint>();
    }

    public Lock.Scope EnterSyncScope() => _syncLock.EnterScope();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void FreezeUpdate()
    {
        if (Atomics.Read(ref _disposed) != default || Atomics.LimitedIncrement(ref _freezeCount, UnsafeHelper.GetMaxValue<nuint>()) != 1)
            return;
        Atomics.Exchange(ref _shouldUpdateWhenUnfreeze, 0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void UnfreezeUpdate(bool forceUpdate)
    {
        if (Atomics.Read(ref _disposed) != default ||
            Atomics.LimitedDecrement(ref _freezeCount, 0) > 0 ||
            (!forceUpdate && Atomics.Exchange(ref _shouldUpdateWhenUnfreeze, default) == default))
            return;
        Update();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual void Update()
    {
        const nuint NeedRefreshBit = 0b01;

        if (Atomics.Read(ref _disposed) != default)
            return;

        Atomics.CompareExchange(ref _shouldUpdateWhenUnfreeze, UnsafeHelper.GetMaxValue<nuint>(), 0);
        if (Atomics.Read(ref _freezeCount) != default ||
            !CheckIsRenderedOnce(Atomics.Or(ref _needRefresh, NeedRefreshBit)))
            return;
        UpdateCore();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected abstract void UpdateCore();

    public void Render(in RegionalRenderingContext context)
    {
        using Lock.Scope scope = EnterSyncScope();

        try
        {
            ResetNeedRefreshFlag();
            if (!RenderCore(in context))
                Update();
        }
        finally
        {
            if (!_enablePartialRendering)
                context.MarkAsDirty();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual bool NeedRefresh() => Atomics.Read(ref _needRefresh) != default;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void ResetNeedRefreshFlag() => Atomics.Exchange(ref _needRefresh, default);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool CheckIsRenderedOnce(ulong requestRedraw)
    {
        const ulong FirstTimeRenderBit = 0b10;
        return (requestRedraw & FirstTimeRenderBit) == 0UL;
    }

    protected abstract bool RenderCore(in RegionalRenderingContext context);

    public override int GetHashCode() => _identifier;

    ~RenderElement() => Dispose(disposing: false);

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (Atomics.Exchange(ref _disposed, Booleans.TrueNativeUnsigned) != default)
            return;

        using Lock.Scope scope = EnterSyncScope();
        DisposeCore(disposing);
    }

    protected virtual void DisposeCore(bool disposing) { }
}
