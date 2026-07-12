using System;

namespace ShioUI.Controls;

partial class ContextMenu
{
    public event EventHandler? ItemClicked;

    public Item[] MenuItems { get; }
}
