using System;

using ShioUI.Graphics.Helpers;
using ShioUI.Layout;

namespace ShioUI.Controls;

partial class ContextMenu
{
    private sealed class DefaultWidthNode : UIElementReferencedNode<ContextMenu>
    {
        public DefaultWidthNode(WeakReference<ContextMenu> reference) : base(reference) { }

        protected override int ComputeCore(ContextMenu element, in LayoutContext context)
        {
            float itemWidth = element._itemSize.Width;
            return MathI.Floor(itemWidth + UIConstants.ElementMargin + RenderingHelper.GetDefaultBorderWidth(element.Window.GetPixelsPerPoint().X) * 2);
        }
    }
}
