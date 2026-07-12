using System.Drawing;
using System.Runtime.CompilerServices;

using ShioUI.Extensions;
using ShioUI.Utils;
using ShioUI.Windows;

namespace ShioUI.Controls;

partial class ContextMenu
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Open(CoreWindow _this, Item[] items)
    {
        if (items.Length <= 0)
            return;

        OpenCore(_this, items, GraphicsUtils.AdjustPoint(_this.WindowToPage(_this.PointToClient(MouseHelper.GetMousePosition()))));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Open(CoreWindow _this, Item[] items, Point location)
    {
        if (items.Length <= 0)
            return;

        OpenCore(_this, items, location);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Open(CoreWindow _this, Item[] items, PointF location)
    {
        if (items.Length <= 0)
            return;

        OpenCore(_this, items, GraphicsUtils.AdjustPoint(location));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Open(CoreWindow _this, UIElement elementRelativeTo, Item[] items, Point location)
    {
        if (items.Length <= 0)
            return;

        OpenCore(_this, items, elementRelativeTo.LocalPageToGlobalPage(elementRelativeTo.LocalToPage(location)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Open(CoreWindow _this, UIElement elementRelativeTo, Item[] items, PointF location)
    {
        if (items.Length <= 0)
            return;

        OpenCore(_this, items,
            elementRelativeTo.LocalPageToGlobalPage(elementRelativeTo.LocalToPage(GraphicsUtils.AdjustPoint(location))));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void OpenCore(CoreWindow _this, Item[] items, Point location)
        => _this.ChangeOverlayElement(new ContextMenu(_this, items, location));
}
