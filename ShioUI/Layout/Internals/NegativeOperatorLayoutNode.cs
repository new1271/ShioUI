using System.Runtime.CompilerServices;

namespace ShioUI.Layout.Internals;

internal sealed class NegativeOperatorLayoutNode : LayoutNode
{
    private readonly LayoutNode _variable;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public NegativeOperatorLayoutNode(LayoutNode variable) => _variable = variable;

    protected override int ComputeCore(in LayoutContext context)
        => -context.GetComputedValue(_variable); 
    
    public sealed class Fractional : FractionalLayoutNode
    {
        private readonly FractionalLayoutNode _variable;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Fractional(FractionalLayoutNode variable) => _variable = variable;

        protected override float ComputeCore(in LayoutContext context)
            => -context.GetComputedValue(_variable);
    }
}
