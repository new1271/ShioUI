using System.Runtime.CompilerServices;

using ShioUI.Utils;

namespace ShioUI.Controls;

partial class PopupContainer
{
    public UIElement? FirstChild
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _children.FirstOrDefault();
    }

    public UIElement? LastChild
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _children.LastOrDefault();
    }

    public UIElementCollection Children
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _children;
    }
}
