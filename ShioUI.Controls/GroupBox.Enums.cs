using System;

namespace ShioUI.Controls;

partial class GroupBox
{
    private enum RedrawType : long
    {
        NoRedraw,
        RedrawTitle,
        RedrawAllContent
    }

    [Flags]
    private enum RenderObjectUpdateFlags : long
    {
        None = 0,
        Title = 0b01,
        Format = 0b11,
        FlagsAllTrue = -1L
    }

    private enum Brush
    {
        BackBrush,
        BorderBrush,
        TextBrush,
        _Last
    }
}
