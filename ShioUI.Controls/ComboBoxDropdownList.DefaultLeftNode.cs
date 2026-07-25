using ShioUI.Extensions;
using ShioUI.Layout;

namespace ShioUI.Controls;

partial class ComboBoxDropdownList
{
    private sealed class DefaultLeftNode : UIElementDependedNode<ComboBoxDropdownList>
    {
        public DefaultLeftNode(ComboBoxDropdownList element) : base(element) { }

        protected override int ComputeCore(ComboBoxDropdownList element, in LayoutContext context)
        {
            ComboBox owner = element._owner;
            return owner.LocalPageToGlobalPage(owner.Location).X;
        }
    }
}
