using System.Runtime.CompilerServices;

using InlineMethod;

using RiceTea.Core.Helpers;

using ShioUI.Layout;

namespace ShioUI.Extensions;

public static class MathHelperExtensions
{
    extension(MathHelper)
    {
        [Inline(InlineBehavior.Keep, export: true)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LayoutNode Min(LayoutNode left, LayoutNode right) => LayoutNode.Min(left, right);

        [Inline(InlineBehavior.Keep, export: true)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FractionalLayoutNode Min(FractionalLayoutNode left, FractionalLayoutNode right) => FractionalLayoutNode.Min(left, right);

        [Inline(InlineBehavior.Keep, export: true)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LayoutNode Max(LayoutNode left, LayoutNode right) => LayoutNode.Min(left, right);

        [Inline(InlineBehavior.Keep, export: true)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FractionalLayoutNode Max(FractionalLayoutNode left, FractionalLayoutNode right) => FractionalLayoutNode.Min(left, right);
    }
}
