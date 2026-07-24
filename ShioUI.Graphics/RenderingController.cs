using System;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;

using ShioUI.Graphics.Internals;
using ShioUI.Graphics.Native.DXGI;

using RiceTea.Core.Helpers;
using RiceTea.Core.Native;
using RiceTea.Core.Windows.Structures;
using RiceTea.Core;

namespace ShioUI.Graphics;

public sealed partial class RenderingController : CriticalFinalizerObject, IDisposable
{
    private readonly IRenderable _target;
    private readonly IFrameWaiter _frameWaiter;
    private readonly RenderingThread _thread;
    private readonly IntPtr _waitForRenderingTrigger;
    private readonly bool _needUpdateFps;

    private ulong _state;
    private nuint _lockedCount, _isSystemBoosting, _disposed;

    public bool NeedUpdateFps => _needUpdateFps;

    public RenderingController(IRenderable target, Rational framesPerSecond)
    {
        _target = target;
        _state = (ulong)RenderingFlags._FlagAllTrue;
        _frameWaiter = CreateFrameWaiter(target, framesPerSecond, out _needUpdateFps);
        _thread = new RenderingThread(this, _frameWaiter);
        _waitForRenderingTrigger = NativeMethods.CreateWaitingHandle(autoReset: false);
        _isSystemBoosting = 0;
    }

    private static IFrameWaiter CreateFrameWaiter(IRenderable target, Rational framesPerSecond, out bool needUpdateFps)
    {
        DXGISwapChain swapChain = target.GetSwapChain();
        if (!swapChain.TryQueryInterface(DXGISwapChain2.IID_IDXGISwapChain2, out DXGISwapChain2? swapChain2))
            goto Fallback;
        try
        {
            IntPtr handle = swapChain2.GetFrameLatencyWaitableObject();
            if (handle == IntPtr.Zero)
                goto Fallback;

            needUpdateFps = false;
            return FrameWaiter.CreateWithWaitHandle(handle);
        }
        finally
        {
            swapChain2.Dispose();
        }
    Fallback:
        needUpdateFps = true;
        return FrameWaiter.CreateWithFramesPerSecond(framesPerSecond);
    }

    public void RequestUpdate(bool force)
    {
        Atomics.Or(ref _state, (ulong)RenderingFlags.BaseFlag | 
            ((ulong)RenderingFlags.RedrawAllFlag & UnsafeHelper.Negate(MathHelper.BooleanToUInt64(force))));
        if (Atomics.Read(ref _lockedCount) != 0UL)
            return;
        _thread.DoRender();
    }

    public void RequestUpdateUnsafe(RenderingFlags flags)
    {
        Atomics.Or(ref _state, (ulong)flags); 

        if (Atomics.Read(ref _lockedCount) != 0UL)
            return;
        _thread.DoRender();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RequestUpdateAndResize()
    {
        Atomics.Or(ref _state, (ulong)RenderingFlags.ResizeAndRedrawAll);
        if (Atomics.Read(ref _lockedCount) != 0UL)
            return;
        _thread.DoRender();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RequestUpdateAndResize(bool temporarily)
    {
        ulong state = (ulong)RenderingFlags.ResizeAndRedrawAll | 
            ((ulong)RenderingFlags._ResizeTemporarilyFlag_Standalone & UnsafeHelper.Negate(MathHelper.BooleanToUInt64(temporarily)));
        Atomics.Or(ref _state, state);
        if (Atomics.Read(ref _lockedCount) != 0UL)
            return;
        _thread.DoRender();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RequestUpdateAndResize(bool temporarily, bool redrawAll)
    {
        ulong state = (ulong)RenderingFlags.Resize |
            ((ulong)RenderingFlags._ResizeTemporarilyFlag_Standalone & UnsafeHelper.Negate(MathHelper.BooleanToUInt64(temporarily))) |
            ((ulong)RenderingFlags.RedrawAll & UnsafeHelper.Negate(MathHelper.BooleanToUInt64(redrawAll)));
        Atomics.Or(ref _state, state);
        if (Atomics.Read(ref _lockedCount) != 0UL)
            return;
        _thread.DoRender();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RenderCore()
    {
        if (Atomics.Read(ref _lockedCount) != 0UL)
            return;
        IntPtr trigger = _waitForRenderingTrigger;
        NativeMethods.ResetWaitingHandle(trigger);
        try
        {
            _target.Render(this);
        }
        finally
        {
            NativeMethods.SetWaitingHandle(trigger);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Lock()
    {
        if (Atomics.Read(ref _disposed) != default ||
            Atomics.LimitedIncrement(ref _lockedCount, UnsafeHelper.GetMaxValue<nuint>()) > 1)
            return;
        _thread.StartNextWaiting();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Unlock()
    {
        if (Atomics.Read(ref _disposed) != default || 
            Atomics.LimitedDecrement(ref _lockedCount, default) > 0 ||
            Atomics.Read(ref _state) == default)
            return;
        _thread.DoRender();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RenderingFlags GetAndResetRenderingFlags()
    {
        _thread.StartNextWaiting();
        return (RenderingFlags)Atomics.Exchange(ref _state, default);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WaitForRendering()
    {
        if (NativeMethods.GetCurrentThreadId() == _thread.RenderingThreadId)
            return;
        NativeMethods.WaitForWaitingHandle(_waitForRenderingTrigger);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetFramesPerSecond(Rational value) => _frameWaiter.FramesPerSecond = value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetSystemBoosting(bool boost)
    {
        nuint value = UnsafeHelper.Negate(MathHelper.BooleanToNativeUnsigned(boost));
        if (Atomics.Exchange(ref _isSystemBoosting, value) == value)
            return;
        DelayedSystemBooster booster = DelayedSystemBooster.Instance;
        if (boost)
            booster.AddRef();
        else
            booster.RemoveRef();
    }

    public bool WaitForExit(int millisecondsTimeout) => _thread.WaitForExit(millisecondsTimeout);

    ~RenderingController() => DisposeCore(disposing: false);

    private void DisposeCore(bool disposing)
    {
        if (Atomics.Exchange(ref _disposed, UnsafeHelper.GetMaxValue<nuint>()) != 0)
            return;
        Atomics.Write(ref _lockedCount, UnsafeHelper.GetMaxValue<nuint>());
        NativeMethods.DestroyWaitingHandle(_waitForRenderingTrigger);
        if (disposing)
        {
            _frameWaiter.Dispose();
            _thread.Dispose();
        }
        if (Atomics.Exchange(ref _isSystemBoosting, default) != default)
            DelayedSystemBooster.Instance.RemoveRef();
    }

    public void Dispose()
    {
        DisposeCore(disposing: true);
        GC.SuppressFinalize(this);
    }
}
