using System.Drawing;
using System.Runtime.CompilerServices;
using System.Threading;

using RiceTea.Core;
using RiceTea.Core.Helpers;
using RiceTea.Core.Threading;

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
        get => MathHelper.ToBoolean(Atomics.Read(ref _drawWhenDisabled));
        set
        {
            uint rawValue = MathHelper.BooleanToUInt32(value);
            if (Atomics.Exchange(ref _drawWhenDisabled, rawValue) == rawValue)
                return;

            Update(ScrollableElementUpdateFlags.All);
        }
    }

    protected ScrollBarType ScrollBarType
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (ScrollBarType)Atomics.Read(ref UnsafeHelper.As<ScrollBarType, uint>(ref _scrollBarType));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            if (Atomics.Exchange(ref UnsafeHelper.As<ScrollBarType, uint>(ref _scrollBarType), (uint)value) == (uint)value)
                return;

            Update(ScrollableElementUpdateFlags.RecalcLayout);
        }
    }

    protected Size SurfaceSize
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            Size result;
            ref readonly nuint versionRef = ref _surfaceSizeVersion;
            nuint version = OptimisticLock.Enter(in versionRef);
            do
            {
                result = BoundsHelper.ConvertUInt64ToSize(_surfaceSizeRaw);
            } while (!OptimisticLock.TryLeave(in versionRef, ref version));
            return result;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            ulong castedValue = BoundsHelper.ConvertSizeToUInt64(value);
            if (Atomics.Exchange(ref _surfaceSizeRaw, castedValue) == castedValue)
                return;
            OptimisticLock.Increase(ref _surfaceSizeVersion);
            Update(ScrollableElementUpdateFlags.RecalcLayout);
        }
    }

    public Point ViewportPoint
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ref readonly ulong resultRef = ref _viewportPointRaw;
            ref readonly nuint versionRef = ref _viewportPointVersion;
            ulong result = OptimisticLock.EnterWithPrimitive(in resultRef, in versionRef, out nuint version);
            while (!OptimisticLock.TryLeaveWithPrimitive(in resultRef, in versionRef, ref result, ref version)) ;
            return BoundsHelper.ConvertUInt64ToPoint(result);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected set
        {
            ulong castedValue = BoundsHelper.ConvertPointToUInt64(value);
            if (Atomics.Exchange(ref _viewportPointRaw, castedValue) == castedValue)
                return;
            OptimisticLock.Increase(ref _viewportPointVersion);
            Update(ScrollableElementUpdateFlags.RecalcScrollBar | ScrollableElementUpdateFlags.TriggerViewportPointChanged | ScrollableElementUpdateFlags.All);
        }
    }

    protected Rectangle ContentBounds
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ulong contentLocation, contentSize;
            ref readonly nuint versionRef = ref _contentBoundsVersion;
            nuint version = OptimisticLock.Enter(in versionRef);
            do
            {
                contentLocation = Volatile.Read(ref _contentLocationRaw);
                contentSize = Volatile.Read(ref _contentSizeRaw);
            } while (!OptimisticLock.TryLeave(in versionRef, ref version));
            return BoundsHelper.ConvertUInt64SlotsToBounds(contentLocation, contentSize);
        }
    }

    protected Point ContentLocation
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ref readonly ulong resultRef = ref _contentLocationRaw;
            ref readonly nuint versionRef = ref _contentBoundsVersion;
            ulong result = OptimisticLock.EnterWithPrimitive(in resultRef, in versionRef, out nuint version);
            while (!OptimisticLock.TryLeaveWithPrimitive(in resultRef, in versionRef, ref result, ref version)) ;
            return BoundsHelper.ConvertUInt64ToPoint(result);
        }
    }

    protected Size ContentSize
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ref readonly ulong resultRef = ref _contentSizeRaw;
            ref readonly nuint versionRef = ref _contentBoundsVersion;
            ulong result = OptimisticLock.EnterWithPrimitive(in resultRef, in versionRef, out nuint version);
            while (!OptimisticLock.TryLeaveWithPrimitive(in resultRef, in versionRef, ref result, ref version)) ;
            return BoundsHelper.ConvertUInt64ToSize(result);
        }
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
