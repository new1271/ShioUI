using System;
using System.Runtime.CompilerServices;

using RiceTea.Core;

namespace ShioUI.Controls;

partial class DropdownBox
{
    partial class List
    {
        public event EventHandler<int>? ItemClicked;

        public new DropdownBox Parent
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
}
