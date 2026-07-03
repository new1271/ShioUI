using System.Runtime.CompilerServices;

namespace ShioUI.Layout.Internals.Fractional;

internal sealed class CustomLayoutNode : FractionalLayoutNode
{
    private readonly CustomComputeDelegate _func;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public CustomLayoutNode(CustomComputeDelegate func) => _func = func;

    protected override float ComputeCore(in LayoutContext context) => _func.Invoke(in context);
}