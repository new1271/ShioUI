using System;

using ShioUI.Graphics.Helpers;
using ShioUI.Layout;

namespace ShioUI.Controls;

partial class ContextMenu
{
    private sealed class DefaultHeightNode : UIElementReferencedNode<ContextMenu>
    {
        public DefaultHeightNode(WeakReference<ContextMenu> reference) : base(reference) { }

        protected override int ComputeCore(ContextMenu element, in LayoutContext context)
        {
            float itemHeight = element._itemSize.Height;
            int length = element._layouts.Length;
            return MathI.Floor(itemHeight * length + RenderingHelper.GetDefaultBorderWidth(element.Window.GetPixelsPerPoint().Y) * 2);
        }
    }
}
