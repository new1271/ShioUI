using System;

namespace ShioUI.Layout.Internals;

internal static class RoundLayoutNode
{
    public sealed class Default : LayoutNode
    {
        private readonly FractionalLayoutNode _node;

        public Default(FractionalLayoutNode node) => _node = node;

        protected override int ComputeCore(in LayoutContext context)
            => MathI.Round(context.GetComputedValue(_node));
    }

    public sealed class Custom : LayoutNode
    {
        private readonly FractionalLayoutNode _node;
        private readonly MidpointRounding _midpointRounding;

        public Custom(FractionalLayoutNode node, MidpointRounding midpointRounding)
        {
            _node = node;
            _midpointRounding = midpointRounding;
        }

        protected override int ComputeCore(in LayoutContext context)
            => MathI.Round(context.GetComputedValue(_node), _midpointRounding);
    }

    public static class Fractional
    {
        public sealed class Default : FractionalLayoutNode
        {
            private readonly FractionalLayoutNode _node;

            public Default(FractionalLayoutNode node) => _node = node;

            protected override float ComputeCore(in LayoutContext context)
                => MathF.Round(context.GetComputedValue(_node));
        }

        public sealed class Custom : FractionalLayoutNode
        {
            private readonly FractionalLayoutNode _node;
            private readonly MidpointRounding _midpointRounding;

            public Custom(FractionalLayoutNode node, MidpointRounding midpointRounding)
            {
                _node = node;
                _midpointRounding = midpointRounding;
            }

            protected override float ComputeCore(in LayoutContext context)
                => MathF.Round(context.GetComputedValue(_node), _midpointRounding);
        }
    }
}
