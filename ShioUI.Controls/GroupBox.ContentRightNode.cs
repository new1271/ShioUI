using System;

using ShioUI.Layout;

namespace ShioUI.Controls;

partial class GroupBox
{
    private sealed class ContentRightNode : UIElementReferencedNode<GroupBox>
    {
        public ContentRightNode(WeakReference<GroupBox> reference) : base(reference) { }

        protected override int ComputeCore(GroupBox element, in LayoutContext context) 
            => GetContentPageRightCore(context.GetComputedValue(element, LayoutProperty.Width));
    }
}
