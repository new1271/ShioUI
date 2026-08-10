using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using InlineIL;

using InlineMethod;

using RiceTea.Core;
using RiceTea.Core.Helpers;
using RiceTea.Core.Structures;
using RiceTea.Core.Threading;

namespace ShioUI.Utils;

public static unsafe partial class BoundsHelper
{
    public static void CopyFromElements(Rect* buffer, ref readonly UIElement? elementRef, nuint length)
    {
        nuint offset = 0;
        for (; length >= 4; length -= 4, buffer += 4, offset += 4)
        {
            DoSingleOperation(buffer, in elementRef, offset);
            DoSingleOperation(buffer + 1, in elementRef, offset + 1);
            DoSingleOperation(buffer + 2, in elementRef, offset + 2);
            DoSingleOperation(buffer + 3, in elementRef, offset + 3);
        }
        Rect* bufferEnd = buffer + length;
        if (buffer >= bufferEnd)
            return;
        DoSingleOperation(buffer++, in elementRef, offset++);
        if (buffer >= bufferEnd)
            return;
        DoSingleOperation(buffer++, in elementRef, offset++);
        if (buffer >= bufferEnd)
            return;
        DoSingleOperation(buffer, in elementRef, offset);

        static void DoSingleOperation(Rect* ptr, ref readonly UIElement? elementRef, nuint offset)
        {
            UIElement? element = UnsafeHelper.AddTypedOffsetAsReadOnly(in elementRef, offset);
            *ptr = element is null ? Rect.Empty : FastGetBounds(element);
        }
    }

    public static void HitTest(Rect* ptr, nuint length, PointF point)
    {
        int x = MathI.Truncate(point.X);
        int y = MathI.Truncate(point.Y);
        if (UnsafeHelper.SizeOf<Rect>() < Limits.GetLimitForVectorizing<int>())
        {
            nuint lengthOfInt32 = length * 4;
            if (lengthOfInt32 > Limits.GetLimitForVectorizing<int>())
            {
                VectorizedHitTest((int*)ptr, lengthOfInt32, x, y);
                return;
            }
        }
        ScalarizedHitTest(ptr, length, x, y);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static partial void VectorizedHitTest(int* ptr, nuint length, int x, int y);

    [Inline(InlineBehavior.Remove)]
    private static void ScalarizedHitTest(Rect* ptr, nuint length, int x, int y)
    {
        for (; length >= 4; length -= 4, ptr += 4)
        {
            DoSingleOperation(ptr, x, y);
            DoSingleOperation(ptr + 1, x, y);
            DoSingleOperation(ptr + 2, x, y);
            DoSingleOperation(ptr + 3, x, y);
        }
        Rect* ptrEnd = ptr + length;
        if (ptr >= ptrEnd)
            return;
        DoSingleOperation(ptr, x, y);
        ptr++;
        if (ptr >= ptrEnd)
            return;
        DoSingleOperation(ptr, x, y);
        ptr++;
        if (ptr >= ptrEnd)
            return;
        DoSingleOperation(ptr, x, y);

        [Inline(InlineBehavior.Remove)]
        static void DoSingleOperation(Rect* ptr, int x, int y)
        {
            bool val = ((ptr->Left <= x) & (ptr->Top <= y)) & ((ptr->Right >= x) & (ptr->Bottom >= y));
            StoreBooleanInRect(ptr, val);
        }
    }

    [Inline(InlineBehavior.Remove)]
    private static void StoreBooleanInRect(void* destination, bool value)
    {
#if DEBUG
        UnsafeHelper.InitBlockUnaligned(destination, 0, UnsafeHelper.SizeOf<Rect>());
#endif
        UnsafeHelper.WriteUnaligned(destination, value);
    }

    [Inline(InlineBehavior.Remove)]
    private static Rect FastGetBounds(UIElement element)
    {
        IL.Push(element);
        IL.Emit.Call(MethodRef.PropertyGet(typeof(UIElement), nameof(UIElement.Bounds)));
        IL.Emit.Call(MethodRef.Operator(typeof(Rect), ConversionOperator.Implicit, ConversionDirection.From, typeof(Rectangle)));
        return IL.Return<Rect>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong AsUInt64(Point value)
        => UnsafeHelper.As<Point, PointLayout>(ref value).Raw;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong AsUInt64(Size value)
        => UnsafeHelper.As<Size, SizeLayout>(ref value).Raw;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point AsPoint(ulong value)
        => UnsafeHelper.As<ulong, PointLayout>(ref value).Point;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Size AsSize(ulong value)
        => UnsafeHelper.As<ulong, SizeLayout>(ref value).Size;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref readonly ulong AsUInt64(ref readonly Point reference)
        => ref UnsafeHelper.As<Point, PointLayout>(ref UnsafeHelper.AsRefIn(in reference)).Raw;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref readonly ulong AsUInt64(ref readonly Size reference) 
        => ref UnsafeHelper.As<Size, SizeLayout>(ref UnsafeHelper.AsRefIn(in reference)).Raw;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref readonly Point AsPoint(ref readonly ulong reference) 
        => ref UnsafeHelper.As<ulong, PointLayout>(ref UnsafeHelper.AsRefIn(in reference)).Point;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref readonly Size AsSize(ref readonly ulong reference) 
        => ref UnsafeHelper.As<ulong, SizeLayout>(ref UnsafeHelper.AsRefIn(in reference)).Size;

    [StructLayout(LayoutKind.Explicit, Size = sizeof(ulong))]
    private readonly struct PointLayout
    {
        [FieldOffset(0)]
        public readonly Point Point;
        [FieldOffset(0)]
        public readonly ulong Raw;

        [FieldOffset(0)]
        public readonly int X;
        [FieldOffset(4)]
        public readonly int Y;
    }

    [StructLayout(LayoutKind.Explicit, Size = sizeof(ulong))]
    private readonly struct SizeLayout
    {
        [FieldOffset(0)]
        public readonly Size Size;
        [FieldOffset(0)]
        public readonly ulong Raw;

        [FieldOffset(0)]
        public readonly int Width;
        [FieldOffset(4)]
        public readonly int Height;
    }
}
