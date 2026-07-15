using System.Runtime.CompilerServices;

using RiceTea.Core.Helpers;

namespace ShioUI.Layout.Internals;

internal sealed class MaxLayoutNode : LayoutNode
{
    private readonly LayoutNode _leftVariable, _rightVariable;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MaxLayoutNode(LayoutNode left, LayoutNode right)
    {
        _leftVariable = left;
        _rightVariable = right;
    }

    protected override int ComputeCore(in LayoutContext context)
        => MathHelper.Max(context.GetComputedValue(_leftVariable), context.GetComputedValue(_rightVariable)); 
    
    public sealed class Fractional : FractionalLayoutNode
    {
        private readonly FractionalLayoutNode _leftVariable, _rightVariable;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Fractional(FractionalLayoutNode left, FractionalLayoutNode right)
        {
            _leftVariable = left;
            _rightVariable = right;
        }

        protected override float ComputeCore(in LayoutContext context)
            => MathHelper.Max(context.GetComputedValue(_leftVariable), context.GetComputedValue(_rightVariable));
    }
}
