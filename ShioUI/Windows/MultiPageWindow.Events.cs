using System.Runtime.InteropServices;

namespace ShioUI.Windows;

public delegate void CurrentPageChangedEventHandler(object? sender, CurrentPageChangedEventArgs args);

partial class MultiPageWindow
{
    public event CurrentPageChangedEventHandler? CurrentPageChanged;

    protected virtual void OnCurrentPageChanged(CurrentPageChangedEventArgs args)
        => CurrentPageChanged?.Invoke(this, args);
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct CurrentPageChangedEventArgs(uint OldPage, uint CurrentPage);