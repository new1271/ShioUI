using System;
using System.Runtime.CompilerServices;

using RiceTea.Core;

namespace ShioUI.Controls;

partial class ComboBoxDropdownList
{
    public event EventHandler<int>? ItemClicked;

    public new ComboBox Parent
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _owner;
    }

    public int SelectedIndex
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Atomics.Read(ref _selectedIndex);
    }
}
