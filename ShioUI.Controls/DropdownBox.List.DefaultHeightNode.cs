using ShioUI.Layout;

using RiceTea.Core;

namespace ShioUI.Controls;

partial class DropdownBox
{
    partial class List
    {
        private sealed class DefaultHeightNode : UIElementDependedNode<List>
        {
            public DefaultHeightNode(List element) : base(element) { }

            protected override int ComputeCore(List element, in LayoutContext context)
                => Atomics.Read(ref element._maxViewHeight);
        }
    }
}