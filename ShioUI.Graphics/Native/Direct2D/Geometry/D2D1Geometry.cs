using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;

using InlineMethod;

using RiceTea.Core.Helpers;
using RiceTea.Core.Native;
using RiceTea.Core.Structures;

namespace ShioUI.Graphics.Native.Direct2D.Geometry;

public unsafe class D2D1Geometry : D2D1Resource
{
    protected new enum MethodTable
    {
        _Start = D2D1Resource.MethodTable._End,
        GetBounds = _Start,
        GetWidenedBounds,
        StrokeContainsPoint,
        FillContainsPoint,
        CompareWithGeometry,
        Simplify,
        Tessellate,
        CombineWithGeometry,
        Outline,
        ComputeArea,
        ComputeLength,
        ComputePointAtLength,
        Widen,
        _End
    }

    public D2D1Geometry() : base() { }

    public D2D1Geometry(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

    /// <summary>
    /// Retrieve the bounds of the geometry.
    /// </summary>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RectF GetBounds() => GetBounds(null);

    /// <inheritdoc cref="GetBounds(Matrix3x2*)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RectF GetBounds(in Matrix3x2 worldTransform)
    {
        fixed (Matrix3x2* pWorldTransform = &worldTransform)
            return GetBounds(pWorldTransform);
    }

    /// <summary>
    /// Retrieve the bounds of the geometry, with an optional applied transform.
    /// </summary>
    [SkipLocalsInit]
    public RectF GetBounds(Matrix3x2* worldTransform)
    {
        RectF result;
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetBounds);
        int hr = ((delegate* unmanaged[Stdcall]<void*, Matrix3x2*, RectF*, int>)functionPointer)(nativePointer, worldTransform, &result);
        AfterUnmanagedCall();
        ThrowHelper.ThrowExceptionForHR(hr);
        return result;
    }

    /// <inheritdoc cref="GetWidenedBounds(float, D2D1StrokeStyle, Matrix3x2*, float)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RectF GetWidenedBounds(float strokeWidth, D2D1StrokeStyle strokeStyle, float flatteningTolerance)
        => GetWidenedBounds(strokeWidth, strokeStyle, null, flatteningTolerance);

    /// <inheritdoc cref="GetWidenedBounds(float, D2D1StrokeStyle, Matrix3x2*, float)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RectF GetWidenedBounds(float strokeWidth, D2D1StrokeStyle strokeStyle, in Matrix3x2 worldTransform, float flatteningTolerance)
    {
        fixed (Matrix3x2* pWorldTransform = &worldTransform)
            return GetWidenedBounds(strokeWidth, strokeStyle, pWorldTransform, flatteningTolerance);
    }

    /// <summary>
    /// Get the bounds of the corresponding geometry after it has been widened or have
    /// an optional pen style applied.
    /// </summary>
    [SkipLocalsInit]
    public RectF GetWidenedBounds(float strokeWidth, D2D1StrokeStyle strokeStyle, Matrix3x2* worldTransform, float flatteningTolerance)
    {
        RectF result;
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetWidenedBounds);
        int hr = ((delegate* unmanaged[Stdcall]<void*, float, void*, Matrix3x2*, float, RectF*, int>)functionPointer)(nativePointer, strokeWidth,
            strokeStyle == null ? null : strokeStyle.NativePointer, worldTransform, flatteningTolerance, &result);
        AfterUnmanagedCall(strokeStyle);
        ThrowHelper.ThrowExceptionForHR(hr);
        return result;
    }

    /// <inheritdoc cref="StrokeContainsPoint(PointF, float, D2D1StrokeStyle, Matrix3x2*, float)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SysBool32 StrokeContainsPoint(PointF point, float strokeWidth, D2D1StrokeStyle strokeStyle, float flatteningTolerance)
        => StrokeContainsPoint(point, strokeWidth, strokeStyle, null, flatteningTolerance);

    /// <inheritdoc cref="StrokeContainsPoint(PointF, float, D2D1StrokeStyle, Matrix3x2*, float)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SysBool32 StrokeContainsPoint(PointF point, float strokeWidth, D2D1StrokeStyle strokeStyle, in Matrix3x2 worldTransform, float flatteningTolerance)
    {
        fixed (Matrix3x2* pWorldTransform = &worldTransform)
            return StrokeContainsPoint(point, strokeWidth, strokeStyle, pWorldTransform, flatteningTolerance);
    }

    /// <summary>
    /// Checks to see whether the corresponding penned and widened geometry contains the
    /// given point.
    /// </summary>
    [SkipLocalsInit]
    public SysBool32 StrokeContainsPoint(PointF point, float strokeWidth, D2D1StrokeStyle strokeStyle, Matrix3x2* worldTransform, float flatteningTolerance)
    {
        SysBool32 result;
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.StrokeContainsPoint);
        int hr = ((delegate* unmanaged[Stdcall]<void*, PointF, float, void*, Matrix3x2*, float, SysBool32*, int>)functionPointer)(nativePointer, point, strokeWidth,
            strokeStyle == null ? null : strokeStyle.NativePointer, worldTransform, flatteningTolerance, &result);
        AfterUnmanagedCall(strokeStyle);
        ThrowHelper.ThrowExceptionForHR(hr);
        return result;
    }

    /// <inheritdoc cref="FillContainsPoint(PointF, Matrix3x2*, float)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SysBool32 FillContainsPoint(PointF point, float flatteningTolerance)
        => FillContainsPoint(point, null, flatteningTolerance);

    /// <inheritdoc cref="FillContainsPoint(PointF, Matrix3x2*, float)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SysBool32 FillContainsPoint(PointF point, in Matrix3x2 worldTransform, float flatteningTolerance)
    {
        fixed (Matrix3x2* pWorldTransform = &worldTransform)
            return FillContainsPoint(point, pWorldTransform, flatteningTolerance);
    }

    /// <summary>
    /// Test whether the given fill of this geometry would contain this point.
    /// </summary>
    [SkipLocalsInit]
    public SysBool32 FillContainsPoint(PointF point, Matrix3x2* worldTransform, float flatteningTolerance)
    {
        SysBool32 result;
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.FillContainsPoint);
        int hr = ((delegate* unmanaged[Stdcall]<void*, PointF, Matrix3x2*, float, SysBool32*, int>)functionPointer)(nativePointer, point, worldTransform, flatteningTolerance, &result);
        AfterUnmanagedCall();
        ThrowHelper.ThrowExceptionForHR(hr);
        return result;
    }

    /// <inheritdoc cref="CompareWithGeometry(D2D1Geometry, Matrix3x2*, float)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public D2D1GeometryRelation CompareWithGeometry(D2D1Geometry geometry, float flatteningTolerance)
        => CompareWithGeometry(geometry, null, flatteningTolerance);

    /// <inheritdoc cref="CompareWithGeometry(D2D1Geometry, Matrix3x2*, float)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public D2D1GeometryRelation CompareWithGeometry(D2D1Geometry geometry, in Matrix3x2 inputGeometryTransform, float flatteningTolerance)
    {
        fixed (Matrix3x2* pInputGeometryTransform = &inputGeometryTransform)
            return CompareWithGeometry(geometry, pInputGeometryTransform, flatteningTolerance);
    }

    /// <summary>
    /// Compare how one geometry intersects or contains another geometry.
    /// </summary>
    [SkipLocalsInit]
    public D2D1GeometryRelation CompareWithGeometry(D2D1Geometry geometry, Matrix3x2* inputGeometryTransform, float flatteningTolerance)
    {
        D2D1GeometryRelation result;
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.CompareWithGeometry);
        int hr = ((delegate* unmanaged[Stdcall]<void*, void*, Matrix3x2*, float, D2D1GeometryRelation*, int>)functionPointer)(nativePointer,
            geometry.NativePointer, inputGeometryTransform, flatteningTolerance, &result);
        ThrowHelper.ThrowExceptionForHR(hr);

        AfterUnmanagedCall(geometry);
        return result;
    }

    /// <inheritdoc cref="Simplify(D2D1GeometrySimplificationOption, Matrix3x2*, float, D2D1SimplifiedGeometrySink)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Simplify(D2D1GeometrySimplificationOption simplificationOption, float flatteningTolerance, D2D1SimplifiedGeometrySink geometrySink)
        => Simplify(simplificationOption, null, flatteningTolerance, geometrySink);

    /// <inheritdoc cref="Simplify(D2D1GeometrySimplificationOption, Matrix3x2*, float, D2D1SimplifiedGeometrySink)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Simplify(D2D1GeometrySimplificationOption simplificationOption, in Matrix3x2 worldTransform,
        float flatteningTolerance, D2D1SimplifiedGeometrySink geometrySink)
    {
        fixed (Matrix3x2* pWorldTransform = &worldTransform)
            Simplify(simplificationOption, pWorldTransform, flatteningTolerance, geometrySink);
    }

    /// <summary>
    /// Converts a geometry to a simplified geometry that has arcs and quadratic beziers
    /// removed.
    /// </summary>
    public void Simplify(D2D1GeometrySimplificationOption simplificationOption, Matrix3x2* worldTransform,
        float flatteningTolerance, D2D1SimplifiedGeometrySink geometrySink)
    {
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.Simplify);
        int hr = ((delegate* unmanaged[Stdcall]<void*, D2D1GeometrySimplificationOption, Matrix3x2*, float, void*, int>)functionPointer)(nativePointer,
            simplificationOption, worldTransform, flatteningTolerance, geometrySink.NativePointer);
        AfterUnmanagedCall(geometrySink);
        ThrowHelper.ThrowExceptionForHR(hr);
    }

    /// <inheritdoc cref="Tessellate(Matrix3x2*, float, D2D1TessellationSink)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Tessellate(float flatteningTolerance, D2D1TessellationSink tessellationSink)
        => Tessellate(null, flatteningTolerance, tessellationSink);

    /// <inheritdoc cref="Tessellate(Matrix3x2*, float, D2D1TessellationSink)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Tessellate(in Matrix3x2 worldTransform, float flatteningTolerance, D2D1TessellationSink tessellationSink)
    {
        fixed (Matrix3x2* pWorldTransform = &worldTransform)
            Tessellate(pWorldTransform, flatteningTolerance, tessellationSink);
    }

    /// <summary>
    /// Tessellates a geometry into triangles.
    /// </summary>
    public void Tessellate(Matrix3x2* worldTransform, float flatteningTolerance, D2D1TessellationSink tessellationSink)
    {
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.Tessellate);
        int hr = ((delegate* unmanaged[Stdcall]<void*, Matrix3x2*, float, void*, int>)functionPointer)(nativePointer,
            worldTransform, flatteningTolerance, tessellationSink.NativePointer);
        AfterUnmanagedCall(tessellationSink);
        ThrowHelper.ThrowExceptionForHR(hr);
    }

    /// <inheritdoc cref="CombineWithGeometry(D2D1Geometry, D2D1CombineMode, Matrix3x2*, float, D2D1SimplifiedGeometrySink)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CombineWithGeometry(D2D1Geometry inputGeometry, D2D1CombineMode combineMode, float flatteningTolerance,
        D2D1SimplifiedGeometrySink geometrySink)
        => CombineWithGeometry(inputGeometry, combineMode, null, flatteningTolerance, geometrySink);

    /// <inheritdoc cref="CombineWithGeometry(D2D1Geometry, D2D1CombineMode, Matrix3x2*, float, D2D1SimplifiedGeometrySink)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CombineWithGeometry(D2D1Geometry inputGeometry, D2D1CombineMode combineMode, in Matrix3x2 inputGeometryTransform,
        float flatteningTolerance, D2D1SimplifiedGeometrySink geometrySink)
    {
        fixed (Matrix3x2* pInputGeometryTransform = &inputGeometryTransform)
            CombineWithGeometry(inputGeometry, combineMode, pInputGeometryTransform, flatteningTolerance, geometrySink);
    }

    /// <summary>
    /// Performs a combine operation between the two geometries to produce a resulting
    /// geometry.
    /// </summary>
    public void CombineWithGeometry(D2D1Geometry inputGeometry, D2D1CombineMode combineMode, Matrix3x2* inputGeometryTransform,
        float flatteningTolerance, D2D1SimplifiedGeometrySink geometrySink)
    {
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.CombineWithGeometry);
        int hr = ((delegate* unmanaged[Stdcall]<void*, void*, D2D1CombineMode, Matrix3x2*, float, void*, int>)functionPointer)(nativePointer, inputGeometry.NativePointer,
            combineMode, inputGeometryTransform, flatteningTolerance, geometrySink.NativePointer);
        AfterUnmanagedCall();
        ThrowHelper.ThrowExceptionForHR(hr);
    }

    /// <inheritdoc cref="Outline(Matrix3x2*, float, D2D1SimplifiedGeometrySink)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Outline(float flatteningTolerance, D2D1SimplifiedGeometrySink geometrySink)
        => Outline(null, flatteningTolerance, geometrySink);

    /// <inheritdoc cref="Outline(Matrix3x2*, float, D2D1SimplifiedGeometrySink)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Outline(in Matrix3x2 worldTransform, float flatteningTolerance, D2D1SimplifiedGeometrySink geometrySink)
    {
        fixed (Matrix3x2* pWorldTransform = &worldTransform)
            Outline(pWorldTransform, flatteningTolerance, geometrySink);
    }

    /// <summary>
    /// Computes the outline of the geometry. The result is written back into a
    /// simplified geometry sink.
    /// </summary>
    public void Outline(Matrix3x2* worldTransform, float flatteningTolerance, D2D1SimplifiedGeometrySink geometrySink)
    {
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.Outline);
        int hr = ((delegate* unmanaged[Stdcall]<void*, Matrix3x2*, float, void*, int>)functionPointer)(nativePointer, worldTransform,
            flatteningTolerance, geometrySink.NativePointer);
        AfterUnmanagedCall(geometrySink);
        ThrowHelper.ThrowExceptionForHR(hr);
    }

    /// <inheritdoc cref="ComputeArea(Matrix3x2*, float)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float ComputeArea(float flatteningTolerance)
        => ComputeArea(null, flatteningTolerance);

    /// <inheritdoc cref="ComputeArea(Matrix3x2*, float)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float ComputeArea(in Matrix3x2 worldTransform, float flatteningTolerance)
    {
        fixed (Matrix3x2* pWorldTransform = &worldTransform)
            return ComputeArea(pWorldTransform, flatteningTolerance);
    }

    /// <summary>
    /// Computes the area of the geometry.
    /// </summary>
    [SkipLocalsInit]
    public float ComputeArea(Matrix3x2* worldTransform, float flatteningTolerance)
    {
        float result;
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.ComputeArea);
        int hr = ((delegate* unmanaged[Stdcall]<void*, Matrix3x2*, float, float*, int>)functionPointer)(nativePointer, worldTransform,
            flatteningTolerance, &result);
        AfterUnmanagedCall();
        ThrowHelper.ThrowExceptionForHR(hr);
        return result;
    }

    /// <inheritdoc cref="ComputeLength(Matrix3x2*, float)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float ComputeLength(float flatteningTolerance)
        => ComputeLength(null, flatteningTolerance);

    /// <inheritdoc cref="ComputeLength(Matrix3x2*, float)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float ComputeLength(in Matrix3x2 worldTransform, float flatteningTolerance)
    {
        fixed (Matrix3x2* pWorldTransform = &worldTransform)
            return ComputeLength(pWorldTransform, flatteningTolerance);
    }

    /// <summary>
    /// Computes the length of the geometry.
    /// </summary>
    [SkipLocalsInit]
    public float ComputeLength(Matrix3x2* worldTransform, float flatteningTolerance)
    {
        float result;
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.ComputeLength);
        int hr = ((delegate* unmanaged[Stdcall]<void*, Matrix3x2*, float, float*, int>)functionPointer)(nativePointer, worldTransform,
            flatteningTolerance, &result);
        AfterUnmanagedCall();
        ThrowHelper.ThrowExceptionForHR(hr);
        return result;
    }

    /// <inheritdoc cref="ComputePointAtLength(float, Matrix3x2*, float, PointF*, PointF*)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ComputePointAtLength(float length, float flatteningTolerance, out PointF point, out PointF unitTangentVector)
    {
        fixed (PointF* pPoint = &point, pUnitTangentVector = &unitTangentVector)
            ComputePointAtLength(length, null, flatteningTolerance, pPoint, pUnitTangentVector);
    }

    /// <inheritdoc cref="ComputePointAtLength(float, Matrix3x2*, float, PointF*, PointF*)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ComputePointAtLength(float length, in Matrix3x2 worldTransform, float flatteningTolerance, out PointF point, out PointF unitTangentVector)
    {
        fixed (Matrix3x2* pWorldTransform = &worldTransform)
        fixed (PointF* pPoint = &point, pUnitTangentVector = &unitTangentVector)
            ComputePointAtLength(length, pWorldTransform, flatteningTolerance, pPoint, pUnitTangentVector);
    }

    /// <summary>
    /// Computes the point and tangent a given distance along the path.
    /// </summary>
    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ComputePointAtLength(float length, Matrix3x2* worldTransform, float flatteningTolerance, PointF* point, PointF* unitTangentVector)
    {
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.ComputePointAtLength);
        int hr = ((delegate* unmanaged[Stdcall]<void*, float, Matrix3x2*, float, PointF*, PointF*, int>)functionPointer)(nativePointer, length, worldTransform,
            flatteningTolerance, point, unitTangentVector);
        AfterUnmanagedCall();
        ThrowHelper.ThrowExceptionForHR(hr);
    }

    /// <inheritdoc cref="Widen(float, D2D1StrokeStyle, Matrix3x2*, float, D2D1GeometrySink)"/>
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Widen(float strokeWidth, D2D1StrokeStyle strokeStyle, float flatteningTolerance, D2D1GeometrySink geometrySink)
        => Widen(strokeWidth, strokeStyle, null, flatteningTolerance, geometrySink);

    /// <inheritdoc cref="Widen(float, D2D1StrokeStyle, Matrix3x2*, float, D2D1GeometrySink)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Widen(float strokeWidth, D2D1StrokeStyle strokeStyle, in Matrix3x2 worldTransform, float flatteningTolerance, D2D1GeometrySink geometrySink)
    {
        fixed (Matrix3x2* pWorldTransform = &worldTransform)
            Widen(strokeWidth, strokeStyle, pWorldTransform, flatteningTolerance, geometrySink);
    }

    /// <summary>
    /// Get the geometry and widen it as well as apply an optional pen style.
    /// </summary>
    public void Widen(float strokeWidth, D2D1StrokeStyle strokeStyle, Matrix3x2* worldTransform, float flatteningTolerance, D2D1GeometrySink geometrySink)
    {
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.Widen);
        int hr = ((delegate* unmanaged[Stdcall]<void*, float, void*, Matrix3x2*, float, void*, int>)functionPointer)(nativePointer, strokeWidth,
            strokeStyle == null ? null : strokeStyle.NativePointer, worldTransform, flatteningTolerance, geometrySink.NativePointer);
        AfterUnmanagedCall(strokeStyle, geometrySink);
        ThrowHelper.ThrowExceptionForHR(hr);
    }
}