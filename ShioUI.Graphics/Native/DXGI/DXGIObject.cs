using System;
using System.Runtime.CompilerServices;
using System.Security;

using RiceTea.Core.Helpers;
using RiceTea.Core.Native;
using RiceTea.Core.Windows.ObjectModels;

namespace ShioUI.Graphics.Native.DXGI;

[SuppressUnmanagedCodeSecurity]
public abstract unsafe class DXGIObject : ComObject
{
    protected new enum MethodTable
    {
        _Start = ComObject.MethodTable._End,
        SetPrivateData = _Start,
        SetPrivateDataInterface,
        GetPrivateData,
        GetParent,
        _End
    }

    public DXGIObject() : base() { }

    public DXGIObject(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

    public void SetPrivateData<T>(in Guid name, T data) where T : unmanaged
        => SetPrivateData(name, unchecked((uint)sizeof(T)), &data);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetPrivateData(in Guid name, uint dataSize, void* pData)
    {
        fixed (Guid* pName = &name)
            SetPrivateData(pName, dataSize, pData);
    }

    public void SetPrivateData(Guid* name, uint dataSize, void* pData)
    {
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.SetPrivateData);
        int hr = ((delegate* unmanaged[Stdcall]<void*, Guid*, uint, void*, int>)functionPointer)(nativePointer, name, dataSize, pData);
        AfterUnmanagedCall();
        ThrowHelper.ThrowExceptionForHR(hr);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetPrivateDataInterface(in Guid name, ComObject value)
    {
        fixed (Guid* pName = &name)
            SetPrivateDataInterface(pName, value);
    }

    public void SetPrivateDataInterface(Guid* name, ComObject value)
    {
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.SetPrivateDataInterface);
        int hr = ((delegate* unmanaged[Stdcall]<void*, Guid*, void*, int>)functionPointer)(nativePointer, name, value == null ? null : value.NativePointer);
        AfterUnmanagedCall();
        ThrowHelper.ThrowExceptionForHR(hr);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetPrivateData(in Guid name, uint* pDataSize, void* pData)
    {
        fixed (Guid* pName = &name)
            GetPrivateData(pName, pDataSize, pData);
    }

    public void GetPrivateData(Guid* name, uint* pDataSize, void* pData)
    {
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetPrivateData);
        int hr = ((delegate* unmanaged[Stdcall]<void*, Guid*, uint*, void*, int>)functionPointer)(nativePointer, name, pDataSize, pData);
        AfterUnmanagedCall();
        ThrowHelper.ThrowExceptionForHR(hr);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T? GetParent<T>(in Guid iid, bool throwException = true) where T : ComObject, new()
    {
        fixed (Guid* riid = &iid)
            return GetParent<T>(riid, throwException);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ComObject? GetParent(in Guid iid, bool throwException = true)
    {
        fixed (Guid* riid = &iid)
            return GetParent(riid, throwException);
    }

    public T? GetParent<T>(Guid* riid, bool throwException = true) where T : ComObject, new()
    {
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetParent);
        int hr = ((delegate* unmanaged[Stdcall]<void*, Guid*, void**, int>)functionPointer)(nativePointer, riid, &nativePointer);
        AfterUnmanagedCall();
        if (throwException)
            ThrowHelper.ThrowExceptionForHR(hr);
        else
            ThrowHelper.ResetPointerForHR(hr, ref nativePointer);
        return FromNativePointer<T>(nativePointer, ReferenceType.Owned);
    }

    public ComObject? GetParent(Guid* riid, bool throwException = true)
    {
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetParent);
        int hr = ((delegate* unmanaged[Stdcall]<void*, Guid*, void**, int>)functionPointer)(nativePointer, riid, &nativePointer);
        AfterUnmanagedCall();
        if (throwException)
            ThrowHelper.ThrowExceptionForHR(hr);
        else
            ThrowHelper.ResetPointerForHR(hr, ref nativePointer);
        return nativePointer == null ? null : new ComObject(nativePointer, ReferenceType.Owned);
    }
}
