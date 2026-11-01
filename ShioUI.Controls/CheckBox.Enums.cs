using System;

namespace ShioUI.Controls;

public enum CheckBoxMode
{
    Regular,
    ToggleSwitch
}

partial class CheckBox
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

    private enum Brush
    {
        BorderBrush,
        BorderHoveredBrush,
        BorderPressedBrush,
        BorderCheckedBrush,
        BorderHoveredCheckedBrush,
        BorderPressedCheckedBrush,
        MarkBrush,
        _CheckBoxRenderingLast,
        ToggleSwitchButtonBrush = _CheckBoxRenderingLast,
        ToggleSwitchButtonHoveredBrush,
        ToggleSwitchButtonPressedBrush,
        ToggleSwitchRailBrush,
        ToggleSwitchRailCheckedBrush,
        _ToggleSwitchRenderingLast,
        ForeBrush = _ToggleSwitchRenderingLast,
        ForeHoveredBrush,
        ForePressedBrush,
        _Last,
    }
}
