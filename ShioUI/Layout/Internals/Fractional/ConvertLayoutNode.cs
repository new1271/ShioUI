namespace ShioUI.Layout.Internals.Fractional;

internal sealed class ConvertLayoutNode : FractionalLayoutNode
{
    private readonly LayoutNode _node;

    public ConvertLayoutNode(LayoutNode node) => _node = node;

    protected override float ComputeCore(in LayoutContext context)
        => context.GetComputedValue(_node);
}
