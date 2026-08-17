using System;

namespace ShioUI.Controls;

public enum GroupBoxMode : uint
{
    Bordered,
    Card,
    _Last
}

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
        Title = 0b001,
        TitleDescription = 0b010,
        Format = 0b111,
        FlagsAllTrue = -1L
    }

    private enum Brush
    {
        BorderBrush,
        TitleBrush,
        TitleDescriptionBrush,
        CardBackBrush,
        CardTitleBrush,
        CardTitleDescriptionBrush,
        _Last
    }
}
