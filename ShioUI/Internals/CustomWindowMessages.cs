using ShioUI.Internals.Native;

namespace ShioUI.Internals;

internal static class CustomWindowMessages
{
    public static readonly uint ShioUI_WindowInvoke 
        = User32.RegisterWindowMessage(nameof(ShioUI_WindowInvoke));

    public static readonly uint ShioUI_DestroyWindowAsync 
        = User32.RegisterWindowMessage(nameof(ShioUI_DestroyWindowAsync));

    public static readonly uint ShioUI_UpdateRefreshRate 
        = User32.RegisterWindowMessage(nameof(ShioUI_UpdateRefreshRate));

    public static readonly uint ShioUI_CallShowDialog
        = User32.RegisterWindowMessage(nameof(ShioUI_CallShowDialog));
}
