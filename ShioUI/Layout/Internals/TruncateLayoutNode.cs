using System;

namespace ShioUI.Layout.Internals;

internal sealed class TruncateLayoutNode : LayoutNode
{
    private readonly FractionalLayoutNode _node;

    public TruncateLayoutNode(FractionalLayoutNode node) => _node = node;

    protected override int ComputeCore(in LayoutContext context)
        => MathI.Truncate(context.GetComputedValue(_node)); 
    
    public sealed class Fractional : FractionalLayoutNode
    {
        private readonly FractionalLayoutNode _node;

        public Fractional(FractionalLayoutNode node) => _node = node;

        protected override float ComputeCore(in LayoutContext context)
            => MathF.Truncate(context.GetComputedValue(_node));
    }
}
