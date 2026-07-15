using System;
using System.Runtime.CompilerServices;

using InlineMethod;

using ShioUI.Layout;

namespace ShioUI.Extensions;

public static class MathIExtensions
{
    extension(MathI)
    {
        [Inline(InlineBehavior.Keep, export: true)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LayoutNode Floor(FractionalLayoutNode node) => LayoutNode.Floor(node);

        [Inline(InlineBehavior.Keep, export: true)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LayoutNode Ceiling(FractionalLayoutNode node) => LayoutNode.Ceiling(node);

        [Inline(InlineBehavior.Keep, export: true)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LayoutNode Round(FractionalLayoutNode node) => LayoutNode.Round(node);

        [Inline(InlineBehavior.Keep, export: true)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LayoutNode Round(FractionalLayoutNode node, MidpointRounding midpointRounding) => LayoutNode.Round(node, midpointRounding);

        [Inline(InlineBehavior.Keep, export: true)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LayoutNode Truncate(FractionalLayoutNode node) => LayoutNode.Truncate(node);
    }
}
