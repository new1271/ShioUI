using System.Runtime.CompilerServices;

namespace ShioUI.Controls;

partial class PopupPanel
{
    public UIElement? Child
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _collection.Value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _collection.Value = value;
    }
}
