using System;

using ShioUI.Graphics.Native.DirectWrite;
using ShioUI.Layout;
using ShioUI.Utils;

namespace ShioUI.Controls;

partial class LabeledToggleControlBase
{
    private sealed class AutoWidthNode : UIElementReferencedNode<LabeledToggleControlBase>
    {
        public AutoWidthNode(WeakReference<LabeledToggleControlBase> reference) : base(reference) { }

        protected override int ComputeCore(LabeledToggleControlBase element, in LayoutContext context)
        {
            string? fontName = element._fontName;
            if (fontName is null)
                return 0;
            using DWriteTextFormat format = SharedResources.DWriteFactory.CreateTextFormat(fontName, element._fontSize);
            return GraphicsUtils.MeasureTextWidthAsInt(element._text, format) + UIConstants.ElementMargin +
                element.CalculateToggleControlWidthForAutoSizing(context.GetComputedValue(element, LayoutProperty.Height));
        }
    }
}
