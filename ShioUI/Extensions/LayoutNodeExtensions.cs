using System.Runtime.CompilerServices;

using ShioUI.Layout;

namespace ShioUI.Extensions;

public static class LayoutNodeExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LayoutNode AsLayoutNode(this int value) => LayoutNode.Fixed(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FractionalLayoutNode AsFractionalLayoutNode(this float value) => FractionalLayoutNode.Fixed(value);
}
