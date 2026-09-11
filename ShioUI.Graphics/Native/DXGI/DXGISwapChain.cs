using System;
using System.Runtime.CompilerServices;
using System.Security;

using InlineMethod;

using RiceTea.Core.Helpers;
using RiceTea.Core.Native;
using RiceTea.Core.Windows.ObjectModels;

namespace ShioUI.Graphics.Native.DXGI;

[SuppressUnmanagedCodeSecurity]
public unsafe class DXGISwapChain : DXGIDeviceSubObject
{
    protected new enum MethodTable
    {
        _Start = DXGIDeviceSubObject.MethodTable._End,
        Present = _Start,
        GetBuffer,
        SetFullscreenState,
        GetFullscreenState,
        GetDesc,
        ResizeBuffers,
        ResizeTarget,
        GetContainingOutput,
        GetFrameStatistics,
        GetLastPresentCount,
        _End
    }

    public DXGISwapChain() : base() { }

    public DXGISwapChain(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

    public DXGISwapChainDescription Description
    {
        [SkipLocalsInit]
        get => GetDesc();
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Present(uint syncInterval, DXGIPresentFlags flags) => ThrowHelper.ThrowExceptionForHR(TryPresent(syncInterval, flags));

    public int TryPresent(uint syncInterval, DXGIPresentFlags flags)
    {
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.Present);
        int result = ((delegate* unmanaged[Stdcall]<void*, uint, DXGIPresentFlags, int>)functionPointer)(nativePointer, syncInterval, flags);
        AfterUnmanagedCall();
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetBuffer<T>(uint buffer, in Guid iid) where T : ComObject, new()
    {
        fixed (Guid* riid = &iid)
            return GetBuffer<T>(buffer, riid);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ComObject GetBuffer(uint buffer, in Guid iid)
    {
        fixed (Guid* riid = &iid)
            return GetBuffer(buffer, riid);
    }

    public T GetBuffer<T>(uint buffer, Guid* riid) where T : ComObject, new()
    {
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetBuffer);
        int hr = ((delegate* unmanaged[Stdcall]<void*, uint, Guid*, void**, int>)functionPointer)(nativePointer, buffer, riid, &nativePointer);
        AfterUnmanagedCall();
        ThrowHelper.ThrowExceptionForHR(hr, nativePointer);
        return FromNativePointer<T>(nativePointer, ReferenceType.Owned)!;
    }

    public ComObject GetBuffer(uint buffer, Guid* riid)
    {
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetBuffer);
        int hr = ((delegate* unmanaged[Stdcall]<void*, uint, Guid*, void**, int>)functionPointer)(nativePointer, buffer, riid, &nativePointer);
        AfterUnmanagedCall();
        ThrowHelper.ThrowExceptionForHR(hr, nativePointer);
        return new ComObject(nativePointer, ReferenceType.Owned);
    }

    [Inline(InlineBehavior.Remove)]
    [SkipLocalsInit]
    private DXGISwapChainDescription GetDesc()
    {
        DXGISwapChainDescription desc;
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetDesc);
        int hr = ((delegate* unmanaged[Stdcall]<void*, DXGISwapChainDescription*, int>)functionPointer)(nativePointer, &desc);
        AfterUnmanagedCall();
        ThrowHelper.ThrowExceptionForHR(hr);
        return desc;
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ResizeBuffers()
        => ResizeBuffers(0, 0, 0, DXGIFormat.Unknown, DXGISwapChainFlags.None);

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ResizeBuffers(uint bufferCount, uint width, uint height)
        => ResizeBuffers(bufferCount, width, height, DXGIFormat.Unknown, DXGISwapChainFlags.None);

    public void ResizeBuffers(uint bufferCount, uint width, uint height, DXGIFormat format, DXGISwapChainFlags flags)
    {
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.ResizeBuffers);
        int hr = ((delegate* unmanaged[Stdcall]<void*, uint, uint, uint, DXGIFormat, DXGISwapChainFlags, int>)functionPointer)(nativePointer,
            bufferCount, width, height, format, flags);
        AfterUnmanagedCall();
        ThrowHelper.ThrowExceptionForHR(hr);
    }
}
