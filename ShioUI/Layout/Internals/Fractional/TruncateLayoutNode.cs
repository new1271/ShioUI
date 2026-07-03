using System;

namespace ShioUI.Layout.Internals.Fractional;

internal sealed class TruncateLayoutNode : LayoutNode
{
    private readonly FractionalLayoutNode _node;

    public TruncateLayoutNode(FractionalLayoutNode node) => _node = node;

    protected override int ComputeCore(in LayoutContext context)
        => MathI.Truncate(context.GetComputedValue(_node));
}
