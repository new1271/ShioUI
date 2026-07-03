using System.Runtime.CompilerServices;

namespace ShioUI.Layout.Internals.Fractional;

internal sealed class NegativeOperatorLayoutNode : FractionalLayoutNode
{
    private readonly FractionalLayoutNode _variable;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public NegativeOperatorLayoutNode(FractionalLayoutNode variable) => _variable = variable;

    protected override float ComputeCore(in LayoutContext context)
        => -context.GetComputedValue(_variable);
}
