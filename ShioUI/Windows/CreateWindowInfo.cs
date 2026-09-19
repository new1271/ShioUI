using System.Runtime.InteropServices;

namespace ShioUI.Windows;

[StructLayout(LayoutKind.Auto)]
public struct CreateWindowInfo
{
    public const int UseDefaultSizeOrLocation = unchecked((int)0x80000000);

    public WindowStyles Styles;
    public WindowExtendedStyles ExtendedStyles;
    public int X;
    public int Y;
    public int Width;
    public int Height;

    public CreateWindowInfo() : this(
        WindowStyles.None,
        WindowExtendedStyles.None,
        UseDefaultSizeOrLocation,
        UseDefaultSizeOrLocation,
        UseDefaultSizeOrLocation,
        UseDefaultSizeOrLocation)
    { }

    public CreateWindowInfo(WindowStyles styles, WindowExtendedStyles extendedStyles) : this(
        styles,
        extendedStyles,
        UseDefaultSizeOrLocation,
        UseDefaultSizeOrLocation,
        UseDefaultSizeOrLocation,
        UseDefaultSizeOrLocation)
    { }

    public CreateWindowInfo(WindowStyles styles, WindowExtendedStyles extendedStyles, int x, int y, int width, int height)
    {
        Styles = styles;
        ExtendedStyles = extendedStyles;
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }
}
