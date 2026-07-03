namespace ShioUI.Layout.Internals;

internal sealed class ToFractionalLayoutNode : FractionalLayoutNode
{
    private readonly LayoutNode _node;

    public ToFractionalLayoutNode(LayoutNode node) => _node = node;

    protected override float ComputeCore(in LayoutContext context)
        => context.GetComputedValue(_node);
}
