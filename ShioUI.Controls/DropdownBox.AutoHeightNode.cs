using System;

using ShioUI.Layout;
using ShioUI.Utils;

namespace ShioUI.Controls;

partial class DropdownBox
{
    private sealed class AutoHeightNode : UIElementDependedNode<DropdownBox>
    {
        public AutoHeightNode(DropdownBox element) : base(element) { }

        protected override int ComputeCore(DropdownBox element, in LayoutContext context)
        {
            string? fontName = element._fontName;
            if (fontName is null)
                return 0;
            return MathI.Ceiling(FontHeightHelper.GetFontHeight(fontName, element._fontSize)) + UIConstants.ElementMargin;
        }
    }
}
