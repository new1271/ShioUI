using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

using RiceTea.Core;
using RiceTea.Core.Buffers;
using RiceTea.Core.Collections;
using RiceTea.Core.Extensions;
using RiceTea.Core.Helpers;
using RiceTea.Core.Native;

using ShioUI.Layout;
using ShioUI.Traits;

namespace ShioUI.Controls;

partial class ListBox : IAutoWidthElement, IAutoHeightElement
{
    public event EventHandler? SelectedIndicesChanged;

    public string[] SelectedItems
    {
        get
        {
            SyncList<string, ObservableList<string>> items = _items;
            using Lock.Scope scope = items.EnterLockScope();
            IList<string> unwrappedItems = items.Items.GetUnderlyingList();
            int count = unwrappedItems.Count;
            if (count <= 0)
                return Array.Empty<string>();
            ArrayPool<string> pool = ArrayPool<string>.Shared;
            string[] buffer = pool.Rent(count);
            try
            {
                CopySelectedItemsToBufferCore(items, ref buffer.AsUnsafeRef().FirstElement, count, out int resultLength);
                if (resultLength <= 0)
                    return Array.Empty<string>();
                string[] result = new string[resultLength];
                Array.Copy(buffer, result, resultLength);
                return result;
            }
            finally
            {
                pool.Return(buffer);
            }
        }
    }

    public unsafe int[] SelectedIndices
    {
        get
        {
            SyncList<string, ObservableList<string>> items = _items;
            using Lock.Scope scope = items.EnterLockScope();
            IList<string> unwrappedItems = items.Items.GetUnderlyingList();
            int count = unwrappedItems.Count;
            if (count <= 0)
                return Array.Empty<int>();
            NativeMemoryPool pool = NativeMemoryPool.Shared;
            TypedNativeMemoryBlock<int> buffer = pool.Rent<int>(count);
            int* ptr = buffer.NativePointer;
            try
            {
                CopySelectedIndicesToBufferCore(ptr, count, out int resultLength);
                if (resultLength <= 0)
                    return Array.Empty<int>();
                int[] result = new int[resultLength];
                fixed (int* destination = result)
                    UnsafeHelper.CopyBlock(destination, ptr, (nuint)resultLength * sizeof(int));
                return result;
            }
            finally
            {
                pool.Return(buffer);
            }
        }
    }

    public int ItemHeight
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Atomics.Read(ref _itemHeight);
    }

    public ListBoxMode Mode
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (ListBoxMode)Atomics.Read(ref UnsafeHelper.As<ListBoxMode, uint>(ref _chooseMode));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            if (Atomics.Exchange(ref UnsafeHelper.As<ListBoxMode, uint>(ref _chooseMode), (uint)value) == (uint)value)
                return;
            Update();
        }
    }

    public IList<string> Items
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _items;
    }

    public float FontSize
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Atomics.Read(ref _fontSize);
        set
        {
            if (Atomics.Exchange(ref _fontSize, value) == value)
                return;
            Atomics.Write(ref _recalcFormat, Booleans.TrueLong);
            Update();
        }
    }

    public string CheckBoxThemePrefix
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _checkBoxThemePrefix;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        init => _checkBoxThemePrefix = value;
    }

    public LayoutNode AutoWidthDefinition
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _autoLayoutDefinitions[0] ??= new AutoWidthNode(this);
    }

    public new LayoutNode AutoHeightDefinition
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _autoLayoutDefinitions[1] ??= new AutoHeightNode(this);
    }
}
