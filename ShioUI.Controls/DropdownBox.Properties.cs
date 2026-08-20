using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

using RiceTea.Core;
using RiceTea.Core.Collections;
using RiceTea.Core.Helpers;

using ShioUI.Layout;
using ShioUI.Traits;

namespace ShioUI.Controls;

partial class DropdownBox : IAutoHeightElement
{
    public event EventHandler? ItemClicked;

    public bool Enabled
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => MathHelper.ToBoolean(Atomics.Read(ref _enabled));
        set
        {
            uint rawValue = MathHelper.BooleanToUInt32(value);
            if (Atomics.Exchange(ref _enabled, rawValue) == rawValue)
                return;
            _state = ButtonTriState.None;
            Update();
        }
    }

    public int SelectedIndex
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Atomics.Read(ref _selectedIndex);
        set
        {
            lock (_selectedIndexLock)
            {
                SyncList<string, ObservableList<string>> items = _items;
                IList<string> unwrappedItems = items.Items.GetUnderlyingList();

                string text;
                using (Lock.Scope scope = items.EnterLockScope())
                {
                    int count = unwrappedItems.Count;
                    if (count <= 0 || value < 0)
                    {
                        value = -1;
                        text = string.Empty;
                    }
                    else
                    {
                        value = MathHelper.Min(value, count - 1);
                        text = unwrappedItems[value];
                    }
                }

                Atomics.Write(ref _selectedIndex, value);
                Text = text;
            }
        }
    }

    public float FontSize
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Atomics.Read(ref _fontSize);
        set
        {
            if (Atomics.Exchange(ref _fontSize, value) == value)
                return;
            Update(RenderObjectUpdateFlags.Format);
        }
    }

    public string Text
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Atomics.Read(ref _text);
        set
        {
            if (ReferenceEquals(Atomics.Exchange(ref _text, value), value))
                return;
            Update(RenderObjectUpdateFlags.Layout);
        }
    }

    public IList<string> Items
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _items;
    }

    public int DropdownListVisibleCount
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Atomics.Read(ref _dropDownListVisibleCount);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => Atomics.Write(ref _dropDownListVisibleCount, value);
    }

    public LayoutNode AutoHeightDefinition
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _autoLayoutDefinitions[0] ??= new AutoHeightNode(this);
    }
}
