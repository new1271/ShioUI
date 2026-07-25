using ShioUI.Layout;

namespace ShioUI.Controls;

partial class ComboBoxDropdownList
{
    private sealed class DefaultWidthNode : UIElementDependedNode<ComboBoxDropdownList>
    {
        public DefaultWidthNode(ComboBoxDropdownList element) : base(element) { }

        protected override int ComputeCore(ComboBoxDropdownList element, in LayoutContext context) 
            => element._owner.Width;
    }
}
