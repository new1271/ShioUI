using System;
using System.Runtime.CompilerServices;
using System.Security;

using InlineMethod;

using RiceTea.Core.Helpers;
using RiceTea.Core.Native;
using RiceTea.Core.Structures;

namespace ShioUI.Graphics.Native.DXGI;

[SuppressUnmanagedCodeSecurity]
public unsafe class DXGIFactory1 : DXGIFactory
{
    public static readonly Guid IID_IDXGIFactory1 = new Guid(0x770aae78, 0xf26f, 0x4dba, 0xa8, 0x29, 0x25, 0x3c, 0x83, 0xd1, 0xb3, 0x87);

    protected new enum MethodTable
    {
        _Start = DXGIFactory.MethodTable._End,
        EnumAdapters1 = _Start,
        IsCurrent,
        _End,
    }

    public DXGIFactory1() : base() { }

    public DXGIFactory1(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static new DXGIFactory1? Create(in Guid iid, bool throwException = true)
    {
        fixed (Guid* riid = &iid)
            return Create(riid, throwException);
    }

    [SkipLocalsInit]
    public static new DXGIFactory1? Create(Guid* riid, bool throwException = true)
    {
        void* factory;
        int hr = DXGI.CreateDXGIFactory1(riid, &factory);
        if (throwException)
            ThrowHelper.ThrowExceptionForHR(hr);
        else
            ThrowHelper.ResetPointerForHR(hr, ref factory);
        return factory == null ? null : new DXGIFactory1(factory, ReferenceType.Owned);
    }

    public SysBool32 IsCurrent => IsCurrentCore();

    public DXGIAdapter1? EnumAdapters1(uint adapter, bool throwException = true)
    {
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.EnumAdapters1);
        int hr = ((delegate* unmanaged[Stdcall]<void*, uint, void**, int>)functionPointer)(nativePointer, adapter, &nativePointer);
        AfterUnmanagedCall();
        if (throwException)
            ThrowHelper.ThrowExceptionForHR(hr);
        else
            ThrowHelper.ResetPointerForHR(hr, ref nativePointer);
        return nativePointer == null ? null : new DXGIAdapter1(nativePointer, ReferenceType.Owned);
    }

    [Inline(InlineBehavior.Remove)]
    private SysBool32 IsCurrentCore()
    {
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.IsCurrent);
        SysBool32 result = ((delegate* unmanaged[Stdcall]<void*, SysBool32>)functionPointer)(nativePointer);
        AfterUnmanagedCall();
        return result;
    }
}
