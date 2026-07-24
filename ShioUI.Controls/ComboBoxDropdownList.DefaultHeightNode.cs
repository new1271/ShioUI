using ShioUI.Layout;

using RiceTea.Core;

namespace ShioUI.Controls;

partial class ComboBoxDropdownList
{
    private sealed class DefaultHeightNode : UIElementDependedNode<ComboBoxDropdownList>
    {
        public DefaultHeightNode(ComboBoxDropdownList element) : base(element) { }

        protected override int ComputeCore(ComboBoxDropdownList element, in LayoutContext context)
            => Atomics.Read(ref element._maxViewHeight);
    }
}
