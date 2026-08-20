using System;
using System.Threading;

using RiceTea.Core;
using RiceTea.Core.Helpers;

using ShioUI.Internals.Native;
using ShioUI.Traits;
using ShioUI.Utils;
using ShioUI.Windows;

namespace ShioUI.Input;

public sealed class InputMethod : IWindowMessageFilter, ICheckableDisposable
{
    private readonly CoreWindow _owner;
    private readonly Lock _syncLock = new();

    private InputMethodContext? _context;
    private IInputMethodHandler? _attachedControl;
    private nuint _disposed;
    private bool _imeStatus;

    public bool IsDisposed => MathHelper.ToBoolean(Atomics.Read(ref _disposed));

    public InputMethodContext? Context => _context;

    public InputMethod(CoreWindow window)
    {
        _owner = window;
        _owner.AddMessageFilter(this);
        _imeStatus = true;
    }

    public void Attach(IInputMethodHandler? control)
    {
        IInputMethodHandler? oldControl = Atomics.Exchange(ref _attachedControl, control);
        if (ReferenceEquals(oldControl, control))
            return;
        if (control is null)
        {
            lock (_syncLock)
                User32.DestroyCaret();
            return;
        }
        IntPtr handle = _owner.Handle;
        if (handle == IntPtr.Zero)
            return;
        lock (_syncLock)
        {
            InputMethodContext? context = _context;
            if (oldControl is null)
                User32.CreateCaret(handle, IntPtr.Zero, 2, 10);
            if (context is null)
            {
                context = InputMethodContext.Create();
                InputMethodContext.Associate(handle, context);
                context.Status = _imeStatus;
                _context = context;
            }
        }
    }

    public void Detach(IInputMethodHandler? control)
    {
        if (control is null || !ReferenceEquals(Atomics.CompareExchange(ref _attachedControl, null, control), control))
            return;
        lock (_syncLock)
        {
            InputMethodContext? context = _context;
            if (context is not null)
            {
                _context = null;
                _imeStatus = context.Status;
                context.Dispose();
            }
            User32.DestroyCaret();
        }
    }

    bool IWindowMessageFilter.TryProcessWindowMessage(IntPtr hwnd, WindowMessage message, nint wParam, nint lParam, out nint result)
    {
        result = 0;

        IInputMethodHandler? attachedControl = Atomics.Read(ref _attachedControl);
        switch (message)
        {
            case WindowMessage.KillFocus:
                if (_context != null)
                    _imeStatus = _context.Status;
                break;
            case WindowMessage.ImeChar:
                return true;
            default:
                break;
        }
        if (attachedControl is null)
            return false;
        switch (message)
        {
            case WindowMessage.Activate:
                {
                    if (wParam != 1 && wParam != 2)
                        break;
                    InputMethodContext? newContext = _context;
                    InputMethodContext.Associate(hwnd, out InputMethodContext oldContext, newContext);
                    if (oldContext != newContext)
                        oldContext?.Dispose();
                }
                break;
            case WindowMessage.ImeSetContext:
                {
                    if (wParam != 1)
                        break;
                    InputMethodContext? newContext = _context;
                    if (newContext is null)
                        break;
                    InputMethodContext.Associate(hwnd, out InputMethodContext oldContext, newContext);
                    if (oldContext != newContext)
                        oldContext?.Dispose();
                    newContext.Status = _imeStatus;
                }
                break;
            case WindowMessage.ImeStartComposition:
                {
                    InputMethodContext? context = _context;
                    if (context is not null && !context.IsEmpty)
                        attachedControl.StartIMEComposition(this, context);
                }
                return true;
            case WindowMessage.ImeEndComposition:
                {
                    InputMethodContext? context = _context;
                    if (context is not null && !context.IsEmpty)
                        attachedControl.EndIMEComposition(this, context);
                }
                return false;
            case WindowMessage.ImeComposition:
                {
                    InputMethodContext? context = _context;
                    if (context is null || context.IsEmpty)
                        break;

                    IMECompositionFlags flags = (IMECompositionFlags)lParam;
                    if ((flags & IMECompositionFlags.CompositionString) > 0)
                    {
                        int cursorPos;
                        if ((flags & IMECompositionFlags.CursorPosition) > 0)
                            cursorPos = context.GetCursorPosition();
                        else
                            cursorPos = -1;
                        attachedControl.OnIMEComposition(this, context, context.GetCompositionString(), flags, cursorPos);
                        return true;
                    }
                    if ((flags & IMECompositionFlags.ResultString) > 0)
                    {
                        attachedControl.OnIMECompositionResult(this, context, context.GetResultString(), flags);
                        return true;
                    }
                }
                return true;
        }
        return false;
    }

    private void DisposeCore()
    {
        if (Atomics.Exchange(ref _disposed, Booleans.TrueNativeUnsigned) != default)
            return;
        _owner.RemoveMessageFilter(this);
        _context?.Dispose();
    }

    public void Dispose()
    {
        DisposeCore();
        GC.SuppressFinalize(this);
    }
}
