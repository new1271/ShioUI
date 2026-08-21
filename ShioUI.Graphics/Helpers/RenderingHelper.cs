using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;

using InlineMethod;

using RiceTea.Core.Structures;

namespace ShioUI.Graphics.Helpers;

public static partial class RenderingHelper
{
    private enum RoundingMethod
    {
        Floor,
        Ceiling,
        Round
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float GetDefaultBorderWidth(float dpiScaleFactor)
        => MathF.Round(dpiScaleFactor, MidpointRounding.AwayFromZero) / dpiScaleFactor;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float FloorInPixel(float valueInPoints, float dpiScaleFactor)
        => RoundInPixelCore(valueInPoints, dpiScaleFactor, RoundingMethod.Floor);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PointF FloorInPixel(Point valueInPoints, Vector2 dpiScaleFactor)
        => RoundInPixelCore(valueInPoints, dpiScaleFactor, RoundingMethod.Floor);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PointF FloorInPixel(PointF valueInPoints, Vector2 dpiScaleFactor)
        => RoundInPixelCore(valueInPoints, dpiScaleFactor, RoundingMethod.Floor);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RectF FloorInPixel(in Rect valueInPoints, Vector2 dpiScaleFactor)
        => RoundInPixelCore(valueInPoints, dpiScaleFactor, RoundingMethod.Floor);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RectF FloorInPixel(in Rectangle valueInPoints, Vector2 dpiScaleFactor)
        => RoundInPixelCore(valueInPoints, dpiScaleFactor, RoundingMethod.Floor);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RectF FloorInPixel(in RectF valueInPoints, Vector2 dpiScaleFactor)
        => RoundInPixelCore(valueInPoints, dpiScaleFactor, RoundingMethod.Floor);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RectF FloorInPixel(in RectangleF valueInPoints, Vector2 dpiScaleFactor)
        => RoundInPixelCore(valueInPoints, dpiScaleFactor, RoundingMethod.Floor);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float CeilingInPixel(float valueInPoints, float dpiScaleFactor)
        => RoundInPixelCore(valueInPoints, dpiScaleFactor, RoundingMethod.Ceiling);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PointF CeilingInPixel(Point valueInPoints, Vector2 dpiScaleFactor)
        => RoundInPixelCore(valueInPoints, dpiScaleFactor, RoundingMethod.Ceiling);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PointF CeilingInPixel(PointF valueInPoints, Vector2 dpiScaleFactor)
        => RoundInPixelCore(valueInPoints, dpiScaleFactor, RoundingMethod.Ceiling);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RectF CeilingInPixel(in Rect valueInPoints, Vector2 dpiScaleFactor)
        => RoundInPixelCore(valueInPoints, dpiScaleFactor, RoundingMethod.Ceiling);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RectF CeilingInPixel(in Rectangle valueInPoints, Vector2 dpiScaleFactor)
        => RoundInPixelCore(valueInPoints, dpiScaleFactor, RoundingMethod.Ceiling);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RectF CeilingInPixel(in RectF valueInPoints, Vector2 dpiScaleFactor)
        => RoundInPixelCore(valueInPoints, dpiScaleFactor, RoundingMethod.Ceiling);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RectF CeilingInPixel(in RectangleF valueInPoints, Vector2 dpiScaleFactor)
        => RoundInPixelCore(valueInPoints, dpiScaleFactor, RoundingMethod.Ceiling);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float RoundInPixel(float valueInPoints, float dpiScaleFactor)
        => RoundInPixelCore(valueInPoints, dpiScaleFactor, RoundingMethod.Round);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PointF RoundInPixel(Point valueInPoints, Vector2 dpiScaleFactor)
        => RoundInPixelCore(valueInPoints, dpiScaleFactor, RoundingMethod.Round);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PointF RoundInPixel(PointF valueInPoints, Vector2 dpiScaleFactor)
        => RoundInPixelCore(valueInPoints, dpiScaleFactor, RoundingMethod.Round);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RectF RoundInPixel(in Rect valueInPoints, Vector2 dpiScaleFactor)
        => RoundInPixelCore(valueInPoints, dpiScaleFactor, RoundingMethod.Round);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RectF RoundInPixel(in Rectangle valueInPoints, Vector2 dpiScaleFactor)
        => RoundInPixelCore(valueInPoints, dpiScaleFactor, RoundingMethod.Round);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RectF RoundInPixel(in RectF valueInPoints, Vector2 dpiScaleFactor)
        => RoundInPixelCore(valueInPoints, dpiScaleFactor, RoundingMethod.Round);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RectF RoundInPixel(in RectangleF valueInPoints, Vector2 dpiScaleFactor)
        => RoundInPixelCore(valueInPoints, dpiScaleFactor, RoundingMethod.Round);

    [Inline(InlineBehavior.Remove)]
    private static float RoundInPixelCore(float valueInPoints, float dpiScaleFactor, [InlineParameter] RoundingMethod method)
    {
        if (dpiScaleFactor == 1.0f)
            return valueInPoints;
        return RoundInPixelCore_Dispatch(valueInPoints, dpiScaleFactor, method);
    }

    [Inline(InlineBehavior.Remove)]
    private static PointF RoundInPixelCore(Point valueInPoints, Vector2 dpiScaleFactor, [InlineParameter] RoundingMethod method)
    {
        if (dpiScaleFactor == Vector2.One)
            return valueInPoints;
        float x = RoundInPixelCore_Dispatch(valueInPoints.X, dpiScaleFactor.X, method);
        float y = RoundInPixelCore_Dispatch(valueInPoints.Y, dpiScaleFactor.Y, method);
        return new PointF(x, y);
    }

    [Inline(InlineBehavior.Remove)]
    private static PointF RoundInPixelCore(PointF valueInPoints, Vector2 dpiScaleFactor, [InlineParameter] RoundingMethod method)
    {
        if (dpiScaleFactor == Vector2.One)
            return valueInPoints;
        float x = RoundInPixelCore_Dispatch(valueInPoints.X, dpiScaleFactor.X, method);
        float y = RoundInPixelCore_Dispatch(valueInPoints.Y, dpiScaleFactor.Y, method);
        return new PointF(x, y);
    }

    [Inline(InlineBehavior.Remove)]
    private static partial RectF RoundInPixelCore(in Rect valueInPoints, Vector2 dpiScaleFactor, [InlineParameter] RoundingMethod method);

    [Inline(InlineBehavior.Remove)]
    private static partial RectF RoundInPixelCore(in Rectangle valueInPoints, Vector2 dpiScaleFactor, [InlineParameter] RoundingMethod method);

    [Inline(InlineBehavior.Remove)]
    private static partial RectF RoundInPixelCore(in RectF valueInPoints, Vector2 dpiScaleFactor, [InlineParameter] RoundingMethod method);

    [Inline(InlineBehavior.Remove)]
    private static partial RectF RoundInPixelCore(in RectangleF valueInPoints, Vector2 dpiScaleFactor, [InlineParameter] RoundingMethod method);

    [Inline(InlineBehavior.Remove)]
    private static float RoundInPixelCore_Dispatch(float valueInPoints, float dpiScaleFactor, [InlineParameter] RoundingMethod method)
        => method switch
        {
            RoundingMethod.Floor => MathF.Floor(valueInPoints * dpiScaleFactor) / dpiScaleFactor,
            RoundingMethod.Ceiling => MathF.Ceiling(valueInPoints * dpiScaleFactor) / dpiScaleFactor,
            RoundingMethod.Round => MathF.Round(valueInPoints * dpiScaleFactor, MidpointRounding.AwayFromZero) / dpiScaleFactor,
            _ => ArgumentOutOfRangeException.Throw<float>(nameof(method))
        };
}
