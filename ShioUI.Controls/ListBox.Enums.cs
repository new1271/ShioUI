namespace ShioUI.Controls;

public enum ListBoxMode
{
    None,
    Any,
    Some
}

partial class ListBox
{
    private enum Brush
    {
        BackBrush,
        BackDisabledBrush,
        BorderBrush,
        TextBrush,
        _Last
    }

    private enum CheckBoxBrush
    {
        BorderBrush,
        BorderHoveredBrush,
        BorderPressedBrush,
        BorderCheckedBrush,
        BorderHoveredCheckedBrush,
        BorderPressedCheckedBrush,
        MarkBrush,
        _Last
    }
}
