using System;

using ShioUI.Extensions;
using ShioUI.Graphics.Helpers;
using ShioUI.Layout;

namespace ShioUI.Controls;

partial class ComboBoxDropdownList
{
    private sealed class DefaultTopNode : UIElementDependedNode<ComboBoxDropdownList>
    {
        public DefaultTopNode(ComboBoxDropdownList element) : base(element) { }

        protected override int ComputeCore(ComboBoxDropdownList element, in LayoutContext context)
        {
            ComboBox owner = element._owner;
            return owner.LocalPageToGlobalPage(owner.Location).Y + owner.Height - 
                MathI.Ceiling(RenderingHelper.GetDefaultBorderWidth(element.Window.GetPixelsPerPoint().Y));
        }
    }
}
