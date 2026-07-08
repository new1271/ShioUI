using System;

using ShioUI.Layout;

namespace ShioUI.Controls;

partial class GroupBox
{
    private sealed class ContentWidthNode : UIElementReferencedNode<GroupBox>
    {
        public ContentWidthNode(WeakReference<GroupBox> reference) : base(reference) { }

        protected override int ComputeCore(GroupBox element, in LayoutContext context) 
            => GetInnerPageWidthCore(context.GetComputedValue(element, LayoutProperty.Width));
    }
}
