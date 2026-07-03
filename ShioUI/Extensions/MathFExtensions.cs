using System;
using System.Runtime.CompilerServices;

using InlineMethod;

using ShioUI.Layout;

namespace ShioUI.Extensions;

public static class MathFExtensions
{
    extension(MathF)
    {
        [Inline(InlineBehavior.Keep, export: true)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FractionalLayoutNode Floor(FractionalLayoutNode node) => FractionalLayoutNode.Floor(node);

        [Inline(InlineBehavior.Keep, export: true)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FractionalLayoutNode Ceiling(FractionalLayoutNode node) => FractionalLayoutNode.Ceiling(node);

        [Inline(InlineBehavior.Keep, export: true)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FractionalLayoutNode Round(FractionalLayoutNode node) => FractionalLayoutNode.Round(node);

        [Inline(InlineBehavior.Keep, export: true)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FractionalLayoutNode Round(FractionalLayoutNode node, MidpointRounding midpointRounding) => FractionalLayoutNode.Round(node, midpointRounding);

        [Inline(InlineBehavior.Keep, export: true)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FractionalLayoutNode Truncate(FractionalLayoutNode node) => FractionalLayoutNode.Truncate(node);
    }
}
