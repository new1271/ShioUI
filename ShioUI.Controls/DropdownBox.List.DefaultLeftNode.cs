using ShioUI.Extensions;
using ShioUI.Layout;

namespace ShioUI.Controls;

partial class DropdownBox
{
    partial class List
    {
        private sealed class DefaultLeftNode : UIElementDependedNode<List>
        {
            public DefaultLeftNode(List element) : base(element) { }

            protected override int ComputeCore(List element, in LayoutContext context)
            {
                DropdownBox owner = element._owner;
                return owner.LocalPageToGlobalPage(owner.Location).X;
            }
        }
    }
}
