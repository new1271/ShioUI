using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using RiceTea.Core;
using RiceTea.Core.Helpers;

using ShioUI.Graphics.Native.DirectWrite;
using ShioUI.Layout;

namespace ShioUI.Controls;

partial class Label : IAutoWidthElement, IAutoHeightElement
{
    public TextAlignment Alignment
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (TextAlignment)Atomics.Read(ref UnsafeHelper.As<TextAlignment, uint>(ref _alignment));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            if (Atomics.Exchange(ref UnsafeHelper.As<TextAlignment, uint>(ref _alignment), (uint)value) == (uint)value)
                return;
            Update(RenderObjectUpdateFlags.Format);
        }
    }

    public float FontSize
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Atomics.Read(ref _fontSize);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            if (Atomics.Exchange(ref _fontSize, value) == value)
                return;
            Update(RenderObjectUpdateFlags.Format);
        }
    }

    public DWriteFontWeight FontWeight
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (DWriteFontWeight)Atomics.Read(ref UnsafeHelper.As<DWriteFontWeight, int>(ref _fontWeight));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            if (Atomics.Exchange(ref UnsafeHelper.As<DWriteFontWeight, int>(ref _fontWeight), (int)value) == (int)value)
                return;
            Update(RenderObjectUpdateFlags.Format);
        }
    }

    public DWriteFontStyle FontStyle
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (DWriteFontStyle)Atomics.Read(ref UnsafeHelper.As<DWriteFontStyle, int>(ref _fontStyle));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            if (Atomics.Exchange(ref UnsafeHelper.As<DWriteFontStyle, int>(ref _fontStyle), (int)value) == (int)value)
                return;
            Update(RenderObjectUpdateFlags.Format);
        }
    }

    public Action<DWriteTextFormat>? PostActionForBuildingFormat
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Atomics.Read(ref _postActionForFormat);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            if (ReferenceEquals(Atomics.Exchange(ref _postActionForFormat, value), value))
                return;
            Update(RenderObjectUpdateFlags.Format);
        }
    }

    [AllowNull]
    public string Text
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Atomics.Read(ref _text);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            value ??= string.Empty;
            if (ReferenceEquals(Atomics.Exchange(ref _text, value), value))
                return;
            Update(RenderObjectUpdateFlags.Layout);
        }
    }

    public bool WordWrap
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => MathHelper.ToBoolean(Atomics.Read(ref _wordWrap));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            uint rawValue = MathHelper.BooleanToUInt32(value);
            if (Atomics.Exchange(ref _wordWrap, rawValue) == rawValue)
                return;
            Update();
        }
    }

    public LayoutNode AutoWidthDefinition
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _autoLayoutDefinitions[0] ??= new AutoWidthNode(this);
    }

    public LayoutNode AutoHeightDefinition
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _autoLayoutDefinitions[1] ??= new AutoHeightNode(this);
    }
}
