using System;

using ShioUI.Layout;

namespace ShioUI.Controls;

partial class ContextMenu
{
    private sealed class DefaultTopNode : UIElementReferencedNode<ContextMenu>
    {
        public DefaultTopNode(WeakReference<ContextMenu> reference) : base(reference) { }

        protected override int ComputeCore(ContextMenu element, in LayoutContext context)
        {
            int result = element._initialLocation.Y;
            int height = context.GetComputedValue(element, LayoutProperty.Height);
            if (result + height >= context.PageSize.Height)
                result = result - height + 1;
            return result;
        }
    }
}
