using System.Runtime.CompilerServices;

namespace ShioUI.Layout.Internals.Fractional;

internal sealed class FixedValueLayoutNode : FractionalLayoutNode
{
    private readonly float _value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FixedValueLayoutNode(float value) => _value = value;

    public float Value
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _value;
    }

    protected override float ComputeCore(in LayoutContext context) => _value;
}
