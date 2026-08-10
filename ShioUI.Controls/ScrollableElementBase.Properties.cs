using System.Drawing;
using System.Runtime.CompilerServices;

using RiceTea.Core;
using RiceTea.Core.Helpers;

using ShioUI.Layout;
using ShioUI.Utils;

namespace ShioUI.Controls;

partial class ScrollableElementBase : IAutoHeightElement
{
    public LayoutNode AutoHeightDefinition
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _autoHeightLayoutNode ??= new AutoHeightNode(this);
    }

    public bool Enabled
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => MathHelper.ToBoolean(Atomics.Read(ref _enabled));
        set
        {
            uint rawValue = MathHelper.BooleanToUInt32(value);
            if (Atomics.Exchange(ref _enabled, rawValue) == rawValue)
                return;

            OnEnableChanged(value);
            Update(ScrollableElementUpdateFlags.RecalcLayout);
        }
    }

    protected bool DrawWhenDisabled
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _drawWhenDisabled;
        init => _drawWhenDisabled = value;
    }

    protected ScrollBarType ScrollBarType
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _scrollBarType;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        init => _scrollBarType = value;
    }

    public Size SurfaceSize
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => BoundsHelper.AsSize(Atomics.Read(ref _surfaceSizeRaw));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected set
        {
            ulong castedValue = BoundsHelper.AsUInt64(value);
            if (Atomics.Exchange(ref _surfaceSizeRaw, castedValue) == castedValue)
                return;
            Update(ScrollableElementUpdateFlags.RecalcLayout);
        }
    }

    public Point ViewportPoint
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => BoundsHelper.AsPoint(Atomics.Read(ref _viewportPointRaw));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected set
        {
            ulong castedValue = BoundsHelper.AsUInt64(value);
            if (Atomics.Exchange(ref _viewportPointRaw, castedValue) == castedValue)
                return;
            Update(ScrollableElementUpdateFlags.RecalcScrollBar | ScrollableElementUpdateFlags.TriggerViewportPointChanged | ScrollableElementUpdateFlags.All);
        }
    }

    protected Rectangle ContentBounds
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _contentBounds.Value;
    }

    protected Point ContentLocation
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => BoundsHelper.FastGetLocation(in _contentBounds);
    }

    protected Size ContentSize
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => BoundsHelper.FastGetSize(in _contentBounds);
    }

    protected bool StickBottom
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _stickBottom;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _stickBottom = value;
    }

    public string ScrollBarThemePrefix
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _scrollBarThemePrefix;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        init => _scrollBarThemePrefix = value;
    }
}
