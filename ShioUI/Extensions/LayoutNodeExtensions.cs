using System;
using System.Runtime.CompilerServices;

using InlineMethod;

using ShioUI.Layout;

namespace ShioUI.Extensions;

public static class LayoutNodeExtensions
{
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static LayoutNode AsLayoutNode(this int value) => LayoutNode.Fixed(value);

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)] 
    public static FractionalLayoutNode AsFractionalLayoutNode(this float value) => FractionalLayoutNode.Fixed(value);
}
