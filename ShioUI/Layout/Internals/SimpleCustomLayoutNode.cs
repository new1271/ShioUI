using System;
using System.Runtime.CompilerServices;

namespace ShioUI.Layout.Internals;

internal sealed class SimpleCustomLayoutNode : LayoutNode
{
    private readonly Func<int> _func;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SimpleCustomLayoutNode(Func<int> func) => _func = func;

    protected override int ComputeCore(in LayoutContext context) => _func.Invoke();
    
    public sealed class Fractional : FractionalLayoutNode
    {
        private readonly Func<float> _func;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Fractional(Func<float> func) => _func = func;

        protected override float ComputeCore(in LayoutContext context) => _func.Invoke();
    }
}