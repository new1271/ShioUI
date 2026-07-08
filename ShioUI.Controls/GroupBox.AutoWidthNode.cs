using System;

using RiceTea.Core.Helpers;

using ShioUI.Graphics.Native.DirectWrite;
using ShioUI.Layout;
using ShioUI.Utils;

namespace ShioUI.Controls;

#pragma warning disable CS0162

partial class GroupBox
{
    private sealed class AutoWidthNode : UIElementReferencedNode<GroupBox>
    {
        public AutoWidthNode(WeakReference<GroupBox> reference) : base(reference) { }

        protected override int ComputeCore(GroupBox element, in LayoutContext context)
        {
            var computeResult = ContainerAutoSizeHelper.Compute(this, element, context,
                    deltaStart: LayoutProperty.Left, deltaEnd: LayoutProperty.Right, initialValue: context.PageSize.Width);
            if (ContentPageLeftPadding == ContentPageRightPadding)
                return computeResult.Result + ContentPageLeftPadding + UIConstants.ElementMargin;
            else
            {
                int mask = (MathHelper.BooleanToInt32(computeResult.ReversedFlow) - 1); // 反相取遮罩
                return computeResult.Result + ((ContentPageLeftPadding & mask) | (ContentPageRightPadding & ~mask)) + UIConstants.ElementMargin;
            }
        }
    }
}
