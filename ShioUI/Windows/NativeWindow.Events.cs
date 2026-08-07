using System;

namespace ShioUI.Windows;

public delegate void ClosingEventHandler(object? sender, ref ClosingEventArgs args);
public delegate void WindowStateChangedEventHandler(object? sender, in WindowStateChangedEventArgs args);

partial class NativeWindow
{
    public event EventHandler? Shown;
    public event EventHandler? Destroyed;
    public event EventHandler? FocusedChanged;
    public event EventHandler? Resizing;
    public event EventHandler? Resized; 
    public event WindowStateChangedEventHandler? WindowStateChanged;
    public event ClosingEventHandler? Closing;
    public event EventHandler? Closed;

    protected virtual void OnShown() => Shown?.Invoke(this, EventArgs.Empty);
    protected virtual void OnDestroyed() => Destroyed?.Invoke(this, EventArgs.Empty);
    protected virtual void OnFocusedChanged() => FocusedChanged?.Invoke(this, EventArgs.Empty);
    protected virtual void OnResizing() => Resizing?.Invoke(this, EventArgs.Empty);
    protected virtual void OnResized() => Resized?.Invoke(this, EventArgs.Empty);
    protected virtual void OnWindowStateChanged(in WindowStateChangedEventArgs args) => WindowStateChanged?.Invoke(this, args);
    protected virtual void OnClosing(ref ClosingEventArgs args) => Closing?.Invoke(this, ref args);
    protected virtual void OnClosed() => Closed?.Invoke(this, EventArgs.Empty);
}

public ref struct ClosingEventArgs
{
    private readonly CloseReason _reason;

    private bool _cancelled;

    public ClosingEventArgs(CloseReason reason, bool cancelled = false)
    {
        _reason = reason;
        _cancelled = cancelled;
    }

    public readonly bool Cancelled => _cancelled;

    public readonly CloseReason Reason => _reason;

    public void SetCancelled(bool cancelled) => _cancelled = cancelled;
}

public readonly ref struct WindowStateChangedEventArgs
{
    private readonly WindowState _oldState, _newState;

    public WindowState OldState => _oldState;

    public WindowState NewState => _newState;

    public WindowStateChangedEventArgs(WindowState oldState, WindowState newState)
    {
        _oldState = oldState;
        _newState = newState;
    }
}
