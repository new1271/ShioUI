using System.Runtime.CompilerServices;

using RiceTea.Core.Helpers;

namespace ShioUI.Layout.Internals;

internal sealed class MinLayoutNode : LayoutNode
{
    private readonly LayoutNode _leftVariable, _rightVariable;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MinLayoutNode(LayoutNode left, LayoutNode right)
    {
        _leftVariable = left;
        _rightVariable = right;
    }

    protected override int ComputeCore(in LayoutContext context)
        => MathHelper.Min(context.GetComputedValue(_leftVariable), context.GetComputedValue(_rightVariable));

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
            => MathHelper.Min(context.GetComputedValue(_leftVariable), context.GetComputedValue(_rightVariable));
    }

}
