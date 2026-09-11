using System.Drawing;
using System.Runtime.CompilerServices;

using RiceTea.Core.Helpers;
using RiceTea.Core.Native;

namespace ShioUI.Graphics.Native.Direct2D.Geometry;

/// <summary>
/// Describes a geometric path that can contain lines, arcs, cubic Bezier curves,
/// and quadratic Bezier curves.
/// </summary>
public unsafe sealed class D2D1GeometrySink : D2D1SimplifiedGeometrySink
{
    private new enum MethodTable
    {
        _Start = D2D1SimplifiedGeometrySink.MethodTable._End,
        AddLine = _Start,
        AddBezier,
        AddQuadraticBezier,
        AddQuadraticBeziers,
        AddArc,
        _End
    }

    public D2D1GeometrySink() : base() { }

    public D2D1GeometrySink(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

    public void AddLine(PointF point)
    {
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.AddLine);
        ((delegate* unmanaged[Stdcall]<void*, PointF, void>)functionPointer)(nativePointer, point);
        AfterUnmanagedCall();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddBezier(in D2D1BezierSegment bezier)
    {
        fixed (D2D1BezierSegment* pBezier = &bezier)
            AddBezier(pBezier);
    }

    public void AddBezier(D2D1BezierSegment* bezier)
    {
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.AddBezier);
        ((delegate* unmanaged[Stdcall]<void*, D2D1BezierSegment*, void>)functionPointer)(nativePointer, bezier);
        AfterUnmanagedCall();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddQuadraticBezier(in D2D1QuadraticBezierSegment bezier)
    {
        fixed (D2D1QuadraticBezierSegment* pBezier = &bezier)
            AddQuadraticBezier(pBezier);
    }

    public void AddQuadraticBezier(D2D1QuadraticBezierSegment* bezier)
    {
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.AddQuadraticBezier);
        ((delegate* unmanaged[Stdcall]<void*, D2D1QuadraticBezierSegment*, void>)functionPointer)(nativePointer, bezier);
        AfterUnmanagedCall();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddQuadraticBeziers(in D2D1QuadraticBezierSegment bezier)
    {
        fixed (D2D1QuadraticBezierSegment* pBeziers = &bezier)
            AddQuadraticBeziers(pBeziers, 1u);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddQuadraticBeziers(params D2D1QuadraticBezierSegment[] beziers)
    {
        fixed (D2D1QuadraticBezierSegment* pBeziers = beziers)
            AddQuadraticBeziers(pBeziers, unchecked((uint)beziers.Length));
    }

    public void AddQuadraticBeziers(D2D1QuadraticBezierSegment* beziers, uint beziersCount)
    {
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.AddQuadraticBeziers);
        ((delegate* unmanaged[Stdcall]<void*, D2D1QuadraticBezierSegment*, uint, void>)functionPointer)(nativePointer, beziers, beziersCount);
        AfterUnmanagedCall();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddArc(in D2D1ArcSegment arc)
    {
        fixed (D2D1ArcSegment* pArc = &arc)
            AddArc(pArc);
    }

    public void AddArc(D2D1ArcSegment* arc)
    {
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.AddArc);
        ((delegate* unmanaged[Stdcall]<void*, D2D1ArcSegment*, void>)functionPointer)(nativePointer, arc);
        AfterUnmanagedCall();
    }
}