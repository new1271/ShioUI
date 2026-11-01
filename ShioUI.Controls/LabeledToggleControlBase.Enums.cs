using System;

namespace ShioUI.Controls;

partial class LabeledToggleControlBase
{
    private enum RedrawType : long
    {
        NoRedraw,
        RedrawCheckBox,
        RedrawAllContent
    }

    [Flags]
    private enum RenderObjectUpdateFlags : long
    {
        None = 0,
        Layout = 0b01,
        Format = 0b11,
        FlagsAllTrue = -1L
    }
}
