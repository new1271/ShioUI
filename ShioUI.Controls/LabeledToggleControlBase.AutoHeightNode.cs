using System;

using ShioUI.Layout;
using ShioUI.Utils;

namespace ShioUI.Controls;

partial class LabeledToggleControlBase
{
    private sealed class AutoHeightNode : UIElementReferencedNode<LabeledToggleControlBase>
    {
        public AutoHeightNode(WeakReference<LabeledToggleControlBase> reference) : base(reference) { }

        protected override int ComputeCore(LabeledToggleControlBase element, in LayoutContext context)
        {
            string? fontName = element._fontName;
            if (fontName is null)
                return 0;
            return MathI.Ceiling(FontHeightHelper.GetFontHeight(fontName, element._fontSize)) + UIConstants.ElementMargin;
        }
    }
}
