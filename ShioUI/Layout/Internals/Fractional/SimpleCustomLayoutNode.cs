using System;
using System.Runtime.CompilerServices;

namespace ShioUI.Layout.Internals.Fractional;

internal sealed class SimpleCustomLayoutNode : FractionalLayoutNode
{
    private readonly Func<float> _func;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SimpleCustomLayoutNode(Func<float> func) => _func = func;

    protected override float ComputeCore(in LayoutContext context) => _func.Invoke();
}