using System;
using System.Runtime.CompilerServices;
using System.Security;

using InlineMethod;

using RiceTea.Core.Helpers;
using RiceTea.Core.Native;
using RiceTea.Core.Windows.ObjectModels;

namespace ShioUI.Graphics.Native.DXGI;

[SuppressUnmanagedCodeSecurity]
public unsafe class DXGIFactory2 : DXGIFactory1
{
    public static readonly Guid IID_IDXGIFactory2 = new Guid(0x50c83a1c, 0xe072, 0x4c48, 0x87, 0xb0, 0x36, 0x30, 0xfa, 0x36, 0xa6, 0xd0);

    protected new enum MethodTable
    {
        _Start = DXGIFactory1.MethodTable._End,
        IsWindowedStereoEnabled = _Start,
        CreateSwapChainForHwnd,
        CreateSwapChainForCoreWindow,
        GetSharedResourceAdapterLuid,
        RegisterStereoStatusWindow,
        RegisterStereoStatusEvent,
        UnregisterStereoStatus,
        RegisterOcclusionStatusWindow,
        RegisterOcclusionStatusEvent,
        UnregisterOcclusionStatus,
        CreateSwapChainForComposition,
        _End,
    }

    public DXGIFactory2() : base() { }

    public DXGIFactory2(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

    public static new DXGIFactory2? Create(in Guid iid, bool throwException = true)
        => Create(DXGICreateFactoryFlags.None, iid, throwException);

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static new DXGIFactory2? Create(Guid* riid, bool throwException = true)
        => Create(DXGICreateFactoryFlags.None, riid, throwException);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DXGIFactory2? Create(DXGICreateFactoryFlags flags, in Guid iid, bool throwException = true)
    {
        fixed (Guid* riid = &iid)
            return Create(flags, riid, throwException);
    }

    [SkipLocalsInit]
    public static DXGIFactory2? Create(DXGICreateFactoryFlags flags, Guid* riid, bool throwException = true)
    {
        void* factory;
        int hr = DXGI.CreateDXGIFactory2(flags, riid, &factory);
        if (throwException)
            ThrowHelper.ThrowExceptionForHR(hr);
        else
            ThrowHelper.ResetPointerForHR(hr, ref factory);
        return factory == null ? null : new DXGIFactory2(factory, ReferenceType.Owned);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public DXGISwapChain1 CreateSwapChainForHwnd(ComObject device, IntPtr handle, in DXGISwapChainDescription1 desc)
    {
        fixed (DXGISwapChainDescription1* pDesc = &desc)
            return CreateSwapChainForHwnd(device, handle, pDesc, null);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public DXGISwapChain1 CreateSwapChainForHwnd(ComObject device, IntPtr handle, in DXGISwapChainDescription1 desc,
        in DXGISwapChainFullscreenDescription fullscreenDesc)
    {
        fixed (DXGISwapChainDescription1* pDesc = &desc)
        fixed (DXGISwapChainFullscreenDescription* pFullscreenDesc = &fullscreenDesc)
            return CreateSwapChainForHwnd(device, handle, pDesc, pFullscreenDesc);
    }

    public DXGISwapChain1 CreateSwapChainForHwnd(ComObject device, IntPtr handle, DXGISwapChainDescription1* pDesc,
        DXGISwapChainFullscreenDescription* pFullscreenDesc)
    {
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.CreateSwapChainForHwnd);
        int hr = ((delegate* unmanaged[Stdcall]<void*, void*, IntPtr, DXGISwapChainDescription1*, DXGISwapChainFullscreenDescription*, void*, void**, int>)functionPointer)(nativePointer,
            device.NativePointer, handle, pDesc, pFullscreenDesc, null, &nativePointer);
        AfterUnmanagedCall(device);
        ThrowHelper.ThrowExceptionForHR(hr, nativePointer);
        return new DXGISwapChain1(nativePointer, ReferenceType.Owned);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public DXGISwapChain1 CreateSwapChainForComposition(ComObject device, in DXGISwapChainDescription1 desc)
    {
        fixed (DXGISwapChainDescription1* pDesc = &desc)
            return CreateSwapChainForComposition(device, pDesc);
    }

    public DXGISwapChain1 CreateSwapChainForComposition(ComObject device, DXGISwapChainDescription1* pDesc)
    {
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.CreateSwapChainForComposition);
        int hr = ((delegate* unmanaged[Stdcall]<void*, void*, DXGISwapChainDescription1*, void*, void**, int>)functionPointer)(nativePointer,
            device.NativePointer, pDesc, null, &nativePointer);
        AfterUnmanagedCall(device);
        ThrowHelper.ThrowExceptionForHR(hr, nativePointer);
        return new DXGISwapChain1(nativePointer, ReferenceType.Owned);
    }
}
