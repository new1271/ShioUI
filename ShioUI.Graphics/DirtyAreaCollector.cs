using System;
using System.Numerics;
using System.Runtime.CompilerServices;

using ShioUI.Graphics.Native.DXGI;

using InlineMethod;

using ShioUI.Graphics.Hosts;

using RiceTea.Core;
using RiceTea.Core.Collections;
using RiceTea.Core.Extensions;
using RiceTea.Core.Helpers;
using RiceTea.Core.Structures;

namespace ShioUI.Graphics;

public sealed partial class DirtyAreaCollector
{
    public static readonly DirtyAreaCollector Empty = new DirtyAreaCollector(null, null);

    private readonly SimpleGraphicsHost? _host;
    private readonly UnwrappableList<RectF>? _list;

    private bool _presentAllMode;

    public DirtyAreaCollector(SimpleGraphicsHost host) :
        this(host is OptimizedGraphicsHost ? new UnwrappableList<RectF>() : null, host)
    { }

    private DirtyAreaCollector(UnwrappableList<RectF>? list, SimpleGraphicsHost? host)
    {
        _list = list;
        _host = host;
    }

    public bool IsEmptyInstance => _host is null && _list is null;

    public bool IsPresentAllMode => _presentAllMode;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasAnyDirtyArea()
    {
        if (_presentAllMode)
            return true;
        UnwrappableList<RectF>? list = _list;
        if (list is null)
            return true;
        return list.Count > 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void MarkAsDirty(in RectF rect) => _list?.Add(rect);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear() => _list?.Clear();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UsePresentAllModeOnce() => _presentAllMode = true;

    public unsafe void Present(Vector2 dpiScaleFactorInversed)
    {
        SimpleGraphicsHost? host = _host;
        if (host is null)
            return;
        UnwrappableList<RectF>? list = _list;
        if (list is null)
        {
            host.Present();
            return;
        }
        if (host is not OptimizedGraphicsHost host1 || Cells.Exchange(ref _presentAllMode, false))
        {
            list.Clear();
            host.Present();
            return;
        }
        RectF[] array = list.Unwrap();
        int count = list.Count;
        if (count <= 0)
            return;
        fixed (RectF* ptr = array)
        {
            uint length = unchecked((uint)count);
            ScaleRects(ptr, length, dpiScaleFactorInversed);
            CleanInvalidRect((Rect*)ptr, length);
            try
            {
                host1.Present(new DXGIPresentParameters(length, (Rect*)ptr));
            }
            finally
            {
                list.Clear();
            }
        }
    }

    public unsafe bool TryPresent(Vector2 dpiScaleFactorInversed)
    {
        SimpleGraphicsHost? host = _host;
        if (host is null)
            return false;
        UnwrappableList<RectF>? list = _list;
        if (list is null)
            return host.TryPresent();
        if (host is not OptimizedGraphicsHost host1 || Cells.Exchange(ref _presentAllMode, false))
        {
            list.Clear();
            return host.TryPresent();
        }
        RectF[] array = list.Unwrap();
        int count = list.Count;
        if (count <= 0)
            return true;
        bool result;
        fixed (RectF* ptr = array)
        {
            uint length = unchecked((uint)count);
            ScaleRects(ptr, length, dpiScaleFactorInversed);
            CleanInvalidRect((Rect*)ptr, length);
            result = host1.TryPresent(new DXGIPresentParameters(length, (Rect*)ptr));
        }
        list.Clear();
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe void CleanInvalidRect(Rect* ptr, nuint length)
    {
        for (; length >= 4; length -= 4, ptr += 4)
        {
            if (!ptr[0].IsValid)
                ptr[0] = default;
            if (!ptr[1].IsValid)
                ptr[1] = default;
            if (!ptr[2].IsValid)
                ptr[2] = default;
            if (!ptr[3].IsValid)
                ptr[3] = default;
        }
        Rect* ptrEnd = ptr + length;
        if (ptr >= ptrEnd)
            return;
        if (!ptr->IsValid)
            *ptr = default;
        ptr++;
        if (ptr >= ptrEnd)
            return;
        if (!ptr->IsValid)
            *ptr = default;
        ptr++;
        if (ptr >= ptrEnd)
            return;
        if (!ptr->IsValid)
            *ptr = default;
    }

    private static unsafe void ScaleRects(RectF* ptr, nuint length, Vector2 dpiScaleFactorInversed)
    {
        DebugHelper.ThrowIf(sizeof(Rect) != sizeof(RectF));

        if (Limits.CheckTypeCanBeVectorized<float>() && Limits.CheckTypeCanBeVectorized<int>())
        {
            nuint limit = Limits.GetLimitForVectorizing<float>();
            if (limit >= UnsafeHelper.SizeOf<RectF>() - 1)
            {
                VectorizedScaleRects(ptr, length, dpiScaleFactorInversed);
                return;
            }
        }
        ScalarizedScaleRects(ref ptr, ref length, dpiScaleFactorInversed);
    }

    [Inline(InlineBehavior.Remove)]
    private static unsafe void VectorizedScaleRects(RectF* ptr, nuint length, Vector2 dpiScaleFactorInversed)
        => VectorizedScaleRects((float*)ptr, length * 4, dpiScaleFactorInversed);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe partial void VectorizedScaleRects(float* ptr, nuint length, Vector2 dpiScaleFactorInversed);

    [Inline(InlineBehavior.Remove)]
    private static unsafe void ScalarizedScaleRects(ref RectF* ptr, ref nuint length, Vector2 dpiScaleFactorInversed)
    {
        (float dpiScaleFactorInversedX, float dpiScaleFactorInversedY) = dpiScaleFactorInversed;
        for (; length >= 4; length -= 4, ptr += 4)
        {
            ScaleRectAndStore(ptr, dpiScaleFactorInversedX, dpiScaleFactorInversedY);
            ScaleRectAndStore(ptr + 1, dpiScaleFactorInversedX, dpiScaleFactorInversedY);
            ScaleRectAndStore(ptr + 2, dpiScaleFactorInversedX, dpiScaleFactorInversedY);
            ScaleRectAndStore(ptr + 3, dpiScaleFactorInversedX, dpiScaleFactorInversedY);
        }
        RectF* ptrEnd = ptr + length;
        if (ptr >= ptrEnd)
            return;
        ScaleRectAndStore(ptr, dpiScaleFactorInversedX, dpiScaleFactorInversedY);
        ptr++;
        if (ptr >= ptrEnd)
            return;
        ScaleRectAndStore(ptr, dpiScaleFactorInversedX, dpiScaleFactorInversedY);
        ptr++;
        if (ptr >= ptrEnd)
            return;
        ScaleRectAndStore(ptr, dpiScaleFactorInversedX, dpiScaleFactorInversedY);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void ScaleRectAndStore(RectF* ptr, float dpiScaleFactorInversedX, float dpiScaleFactorInversedY)
        {
            ScaleAndStore((float*)ptr, dpiScaleFactorInversedX);
            ScaleAndStore(((float*)ptr) + 1, dpiScaleFactorInversedY);
            ScaleAndStore(((float*)ptr) + 2, dpiScaleFactorInversedX);
            ScaleAndStore(((float*)ptr) + 3, dpiScaleFactorInversedY);
        }

        [Inline(InlineBehavior.Remove)]
        static void ScaleAndStore(float* ptr, float dpiScaleFactorInversed) => *(int*)ptr = MathI.Round(*ptr * dpiScaleFactorInversed, MidpointRounding.AwayFromZero);
    }
}
