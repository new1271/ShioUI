using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using RiceTea.Core;
using RiceTea.Core.Extensions;
using RiceTea.Core.Helpers;
using RiceTea.Core.Structures;

using ShioUI.Internals.Native;
using ShioUI.Traits;
using ShioUI.Utils;
using ShioUI.Windows;

namespace ShioUI.Accessibility;

public sealed partial class ToolTip : IWindowMessageFilter, ICheckableDisposable
{
    public static readonly int DefaultShowDelay = (int)SystemParameters.DoubleClickTime;

    private readonly ConditionalWeakTable<UIElement, string> _elementTooltipTable = new();
    private readonly Lock _syncLock = new();
    private readonly CoreWindow _owner;
    private readonly Timer _timer;
    private readonly string _themePrefix = "app.tooltip";

    private Window? _activeWindow;
    private ulong _recordedPoint;
    private int _showDelay = DefaultShowDelay;
    private nuint _disposed, _mouseDownState;

    public bool IsDisposed => MathHelper.ToBoolean(Atomics.Read(ref _disposed));

    public ToolTip(CoreWindow owner)
    {
        _owner = owner;
        _timer = new Timer(static state => (state as ToolTip)?.Timer_Tick(), this, Timeout.Infinite, Timeout.Infinite);
        owner.AddMessageFilter(this);
    }

    public void Attach(UIElement element, string text)
    {
        lock (_syncLock)
        {
            ConditionalWeakTable<UIElement, string> elementTooltipTable = _elementTooltipTable;
#if NET8_0_OR_GREATER
            if (!elementTooltipTable.TryAdd(element, text))
            {
                elementTooltipTable.Remove(element);
                elementTooltipTable.Add(element, text);
            }
#else
            try
            {
                elementTooltipTable.Add(element, text);
            }
            catch (Exception)
            {
                elementTooltipTable.Remove(element);
                elementTooltipTable.Add(element, text);
            }
#endif
        }
    }

    public void Detach(UIElement element)
    {
        lock (_syncLock)
            _elementTooltipTable.Remove(element);
    }

    public void Show(Point point) => ShowCore(_owner.WindowToScreen(point));

    public void Show(PointF point) => ShowCore(_owner.WindowToScreen(point));

    public void Show() => ShowCore(MouseHelper.GetMousePosition());

    public void Close()
    {
        lock (_syncLock)
        {
            DisposeHelper.SwapDispose(ref _activeWindow);
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }
    }

    unsafe bool IWindowMessageFilter.TryProcessWindowMessage(IntPtr hwnd, WindowMessage message, nint wParam, nint lParam, out nint result)
    {
        Point point;
        switch (message)
        {
            case WindowMessage.Activate:
                if (wParam == default)
                    goto Skip;
                break;
            case WindowMessage.NCMouseMove:
                if (_mouseDownState != default)
                    goto Skip;
                point = UnsafeHelper.As<Words, Point16>(lParam.GetWords()).ToPoint32();
                goto Valid;
            case WindowMessage.MouseMove:
                if (_mouseDownState != default)
                    goto Skip;
                point = UnsafeHelper.As<Words, Point16>(lParam.GetWords()).ToPoint32();
                if (User32.ClientToScreen(hwnd, &point))
                    goto Valid;
                break;
            case WindowMessage.MouseLeave:
                goto Skip;
            case WindowMessage.LeftButtonDown:
            case WindowMessage.RightButtonDown:
            case WindowMessage.MiddleButtonDown:
            case WindowMessage.XButtonDown:
                _mouseDownState = (nuint)wParam;
                goto Skip;
            case WindowMessage.LeftButtonUp:
            case WindowMessage.RightButtonUp:
            case WindowMessage.MiddleButtonUp:
            case WindowMessage.XButtonUp:
                _mouseDownState = (nuint)wParam;
                goto Skip;
            default:
                break;
        }
        goto Tail;

    Valid:
        Point lastPoint = BoundsHelper.AsPoint(_recordedPoint);
        if (MathHelper.Abs(lastPoint.X - point.X) > SystemParameters.MouseHoverWidth ||
            MathHelper.Abs(lastPoint.Y - point.Y) > SystemParameters.MouseHoverHeight)
        {
            lock (_syncLock)
            {
                DisposeHelper.SwapDispose(ref _activeWindow);
                _recordedPoint = BoundsHelper.AsUInt64(point);
                _timer.Change(Atomics.Read(ref _showDelay), Timeout.Infinite);
            }
        }
        goto Tail;

    Skip:
        Close();
        goto Tail;

    Tail:
        result = 0;
        return false;
    }

    private async void Timer_Tick()
    {
        Point lastPoint = BoundsHelper.AsPoint(Atomics.Read(ref _recordedPoint));
        Point screenPoint = MouseHelper.GetMousePosition();
        if (MathHelper.Abs(lastPoint.X - screenPoint.X) > SystemParameters.MouseHoverWidth ||
            MathHelper.Abs(lastPoint.Y - screenPoint.Y) > SystemParameters.MouseHoverHeight)
            return;
        ShowCore(screenPoint);
    }

    private async void ShowCore(Point screenPoint)
    {
        CoreWindow owner = _owner;
        if (owner.IsDisposed || await WindowMessageLoop.InvokeTaskAsync(static () => User32.GetActiveWindow()) != owner.Handle)
            return;
        PointF point = owner.ScreenToWindow(screenPoint);
        if ((!owner.TryGetElementFromPoint(owner.WindowToPage(point), out UIElement? element, out PointF localPoint) || !TryGetToolTipTextForElement(element, localPoint, out string? result)) &&
            !TryGetToolTipTextForWindow(owner, point, out result))
            return;

        lock (_syncLock)
        {
            Window window = new Window(this, result, 500, screenPoint);
            window.Show();
            DisposeHelper.SwapDispose(ref _activeWindow, window);
        }
    }

    private bool TryGetToolTipTextForElement(UIElement element, PointF point, [NotNullWhen(true)] out string? result)
    {
        if (element is IToolTipHandler handler && handler.TryGetToolTipText(point, out result))
            return true;
        lock (_syncLock)
            return _elementTooltipTable.TryGetValue(element, out result);
    }

    private static bool TryGetToolTipTextForWindow(CoreWindow window, PointF point, [NotNullWhen(true)] out string? result)
    {
        if (window is IToolTipHandler handler)
            return handler.TryGetToolTipText(point, out result);
        result = null;
        return false;
    }

    private void DisposeCore()
    {
        if (Atomics.Exchange(ref _disposed, Booleans.TrueNativeUnsigned) != default)
            return;
        _owner.RemoveMessageFilter(this);
        lock (_syncLock)
            DisposeHelper.SwapDispose(ref _activeWindow);
    }

    public void Dispose()
    {
        DisposeCore();
        GC.SuppressFinalize(this);
    }
}
