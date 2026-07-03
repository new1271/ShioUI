using System;

namespace ShioUI.Layout.Internals;

internal sealed class FloorLayoutNode : LayoutNode
{
    private readonly FractionalLayoutNode _node;

    public FloorLayoutNode(FractionalLayoutNode node) => _node = node;

    protected override int ComputeCore(in LayoutContext context)
        => MathI.Floor(context.GetComputedValue(_node));

    public sealed class Fractional : FractionalLayoutNode
    {
        private readonly FractionalLayoutNode _node;

        public Fractional(FractionalLayoutNode node) => _node = node;

        protected override float ComputeCore(in LayoutContext context)
            => MathF.Floor(context.GetComputedValue(_node));
    }
}
