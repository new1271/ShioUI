using System;
using System.Runtime.CompilerServices;

using RiceTea.Core;
using RiceTea.Core.Extensions;
using RiceTea.Core.Helpers;

using ShioUI.Layout;
using ShioUI.Traits;

namespace ShioUI.Controls;

partial class CheckBox : IAutoWidthElement, IAutoHeightElement
{
    public event EventHandler? CheckedChanged;

    public bool RightSided
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _isRightSided;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        init => _isRightSided = value;
    }

    public CheckBoxMode Mode
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _mode;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        init => _mode = value;
    }

    public bool Checked
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => MathHelper.ToBoolean(Atomics.Read(ref _checked));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            uint rawValue = MathHelper.BooleanToUInt32(value);
            if (Atomics.Exchange(ref _checked, rawValue) == rawValue)
                return;
            CheckedChanged?.Invoke(this, EventArgs.Empty);
            Update();
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
            DisposeHelper.SwapDisposeAtomic(ref _layout);
            Update();
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
            Update();
        }
    }

    public LayoutNode AutoWidthDefinition
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _autoLayoutDefinitions.AsUnsafeRef()[0] ??= new AutoWidthNode(GetWeakReference());
    }

    public LayoutNode AutoHeightDefinition
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _autoLayoutDefinitions.AsUnsafeRef()[1] ??= new AutoHeightNode(GetWeakReference());
    }
}
