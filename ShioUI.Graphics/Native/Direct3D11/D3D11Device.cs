using System;
using System.Runtime.CompilerServices;
using System.Security;

using InlineMethod;

using ShioUI.Graphics.Native.Direct3D;
using ShioUI.Graphics.Native.DXGI;

using RiceTea.Core.Helpers;
using RiceTea.Core.Native;
using RiceTea.Core.Windows.ObjectModels;

namespace ShioUI.Graphics.Native.Direct3D11;

[SuppressUnmanagedCodeSecurity]
public unsafe sealed class D3D11Device : ComObject
{
    public static readonly Guid IID_ID3D11Device = new Guid(0xdb6f6ddb, 0xac77, 0x4e88, 0x82, 0x53, 0x81, 0x9d, 0xf9, 0xbb, 0xf1, 0x40);

    public D3D11Device() : base() { }

    public D3D11Device(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int TryCreate(DXGIAdapter? adapter, D3DDriverType driverType, IntPtr software, D3D11CreateDeviceFlags createDeviceFlags,
        out D3D11Device? result)
        => TryCreate(adapter, driverType, software, createDeviceFlags, null, 0u, out result);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int TryCreate(DXGIAdapter? adapter, D3DDriverType driverType, IntPtr software,
        D3D11CreateDeviceFlags createDeviceFlags, D3DFeatureLevel[]? featureLevels,
        out D3D11Device? result)
    {
        fixed (D3DFeatureLevel* ptr = featureLevels)
            return TryCreate(adapter, driverType, software, createDeviceFlags, ptr, MathHelper.MakeUnsigned(featureLevels?.Length ?? 0), out result);
    }

    [SkipLocalsInit]
    public static int TryCreate(DXGIAdapter? adapter, D3DDriverType driverType, IntPtr software,
        D3D11CreateDeviceFlags createDeviceFlags, D3DFeatureLevel* featureLevels, uint featureLevelCount,
        out D3D11Device? result)
    {
        void* device;
        int hr = D3D11.D3D11CreateDevice(adapter == null ? null : adapter.NativePointer, driverType, software,
             createDeviceFlags, featureLevels, featureLevelCount, D3D11.D3D11_SDK_VERSION, &device, null, null);
        GC.KeepAlive(adapter);
        if (hr < 0 || device is null)
            result = null;
        else
            result = new D3D11Device(device, ReferenceType.Owned);
        return hr;
    }
}
