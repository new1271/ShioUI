using System.Runtime.CompilerServices;

namespace ShioUI.Layout.Internals;

internal sealed class FixedValueLayoutNode : LayoutNode
{
    private readonly int _value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FixedValueLayoutNode(int value) => _value = value;

    public int Value
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _value;
    }

    protected override int ComputeCore(in LayoutContext context) => _value;

    public sealed class Fractional : FractionalLayoutNode
    {
        private readonly float _value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Fractional(float value) => _value = value;

        public float Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _value;
        }

        protected override float ComputeCore(in LayoutContext context) => _value;
    }
}
