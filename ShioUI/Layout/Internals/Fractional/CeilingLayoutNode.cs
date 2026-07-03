using System;

namespace ShioUI.Layout.Internals.Fractional;

internal sealed class CeilingLayoutNode : LayoutNode
{
    private readonly FractionalLayoutNode _node;

    public CeilingLayoutNode(FractionalLayoutNode node) => _node = node;

    protected override int ComputeCore(in LayoutContext context)
        => MathI.Ceiling(context.GetComputedValue(_node));
}
