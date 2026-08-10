using ShioUI.Layout;

namespace ShioUI.Controls;

partial class DropdownBox
{
    partial class List
    {
        private sealed class DefaultWidthNode : UIElementDependedNode<List>
        {
            public DefaultWidthNode(List element) : base(element) { }

            protected override int ComputeCore(List element, in LayoutContext context)
                => element._owner.Width;
        }
    }
}