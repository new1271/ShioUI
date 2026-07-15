using System;

namespace ShioUI.Layout.Internals;

internal sealed class CeilingLayoutNode : LayoutNode
{
    private readonly FractionalLayoutNode _node;

    public CeilingLayoutNode(FractionalLayoutNode node) => _node = node;

    protected override int ComputeCore(in LayoutContext context)
        => MathI.Ceiling(context.GetComputedValue(_node)); 
    
    public sealed class Fractional : FractionalLayoutNode
    {
        private readonly FractionalLayoutNode _node;

        public Fractional(FractionalLayoutNode node) => _node = node;

        protected override float ComputeCore(in LayoutContext context)
            => MathF.Ceiling(context.GetComputedValue(_node));
    }
}
