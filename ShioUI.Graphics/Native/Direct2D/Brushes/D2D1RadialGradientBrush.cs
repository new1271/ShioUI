using System.Drawing;
using System.Runtime.CompilerServices;

using InlineMethod;

using RiceTea.Core.Native;

namespace ShioUI.Graphics.Native.Direct2D.Brushes;

public unsafe sealed class D2D1RadialGradientBrush : D2D1Brush
{
    private new enum MethodTable
    {
        _Start = D2D1Brush.MethodTable._End,
        SetCenter = _Start,
        SetGradientOriginOffset,
        SetRadiusX,
        SetRadiusY,
        GetCenter,
        GetGradientOriginOffset,
        GetRadiusX,
        GetRadiusY,
        GetGradientStopCollection,
        _End
    }

    public D2D1RadialGradientBrush() : base() { }

    public D2D1RadialGradientBrush(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

    /// <summary>
    /// Gets or sets the center of the radial gradient. <br/>
    /// This will be in local coordinates and will not depend on the geometry being filled.
    /// </summary>
    public PointF Center
    {
        get => GetCenter();
        set => SetCenter(value);
    }

    /// <summary>
    /// Gets or sets offset of the origin relative to the radial gradient center.
    /// </summary>
    public PointF GradientOriginOffset
    {
        get => GetGradientOriginOffset();
        set => SetGradientOriginOffset(value);
    }

    public float RadiusX
    {
        get => GetRadiusX();
        set => SetRadiusX(value);
    }

    public float RadiusY
    {
        get => GetRadiusY();
        set => SetRadiusY(value);
    }

    public D2D1GradientStopCollection? GradientStopCollection => GetGradientStopCollection();

    [Inline(InlineBehavior.Remove)]
    private void SetCenter(PointF startPoint)
    {
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.SetCenter);
        ((delegate* unmanaged[Stdcall]<void*, PointF, void>)functionPointer)(nativePointer, startPoint);
        AfterUnmanagedCall();
    }

    [Inline(InlineBehavior.Remove)]
    private void SetGradientOriginOffset(PointF endPoint)
    {
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.SetGradientOriginOffset);
        ((delegate* unmanaged[Stdcall]<void*, PointF, void>)functionPointer)(nativePointer, endPoint);
        AfterUnmanagedCall();
    }

    [Inline(InlineBehavior.Remove)]
    private void SetRadiusX(float radiusX)
    {
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.SetRadiusX);
        ((delegate* unmanaged[Stdcall]<void*, float, void>)functionPointer)(nativePointer, radiusX);
        AfterUnmanagedCall();
    }

    [Inline(InlineBehavior.Remove)]
    private void SetRadiusY(float radiusY)
    {
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.SetRadiusY);
        ((delegate* unmanaged[Stdcall]<void*, float, void>)functionPointer)(nativePointer, radiusY);
        AfterUnmanagedCall();
    }

    [SkipLocalsInit]
    [Inline(InlineBehavior.Remove)]
    private PointF GetCenter()
    {
        PointF result;
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetCenter);
        ((delegate* unmanaged[Stdcall]<void*, PointF*, void>)functionPointer)(nativePointer, &result);
        AfterUnmanagedCall();
        return result;
    }

    [SkipLocalsInit]
    [Inline(InlineBehavior.Remove)]
    private PointF GetGradientOriginOffset()
    {
        PointF result;
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetGradientOriginOffset);
        ((delegate* unmanaged[Stdcall]<void*, PointF*, void>)functionPointer)(nativePointer, &result);
        AfterUnmanagedCall();
        return result;
    }

    [Inline(InlineBehavior.Remove)]
    private float GetRadiusX()
    {
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetRadiusX);
        float result = ((delegate* unmanaged[Stdcall]<void*, float>)functionPointer)(nativePointer);
        AfterUnmanagedCall();
        return result;
    }

    [Inline(InlineBehavior.Remove)]
    private float GetRadiusY()
    {
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetRadiusY);
        float result = ((delegate* unmanaged[Stdcall]<void*, float>)functionPointer)(nativePointer);
        AfterUnmanagedCall();
        return result;
    }

    [Inline(InlineBehavior.Remove)]
    private D2D1GradientStopCollection? GetGradientStopCollection()
    {
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetGradientStopCollection);
        ((delegate* unmanaged[Stdcall]<void*, void**, void>)functionPointer)(nativePointer, &nativePointer);
        AfterUnmanagedCall();
        return nativePointer == null ? null : new D2D1GradientStopCollection(nativePointer, ReferenceType.Owned);
    }
}
