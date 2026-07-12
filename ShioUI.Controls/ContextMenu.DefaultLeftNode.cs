using System;
using System.Drawing;

using ShioUI.Layout;

namespace ShioUI.Controls;

partial class ContextMenu
{
    private sealed class DefaultLeftNode : UIElementReferencedNode<ContextMenu>
    {
        public DefaultLeftNode(WeakReference<ContextMenu> reference) : base(reference) { }

        protected override int ComputeCore(ContextMenu element, in LayoutContext context)
        {
            int result = element._initialLocation.X;
            int width = context.GetComputedValue(element, LayoutProperty.Width);
            if (result + width >= context.PageSize.Width)
                result = result - width + 1;
            return result;
        }
    }
}
