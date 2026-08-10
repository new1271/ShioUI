using System;

using ShioUI.Extensions;
using ShioUI.Graphics.Helpers;
using ShioUI.Layout;

namespace ShioUI.Controls;

partial class DropdownBox
{
    partial class List
    {
        private sealed class DefaultTopNode : UIElementDependedNode<List>
        {
            public DefaultTopNode(List element) : base(element) { }

            protected override int ComputeCore(List element, in LayoutContext context)
            {
                DropdownBox owner = element._owner;
                return owner.LocalPageToGlobalPage(owner.Location).Y + owner.Height -
                    MathI.Ceiling(RenderingHelper.GetDefaultBorderWidth(element.Window.GetPixelsPerPoint().Y));
            }
        }
    }
}
