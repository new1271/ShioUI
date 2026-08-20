using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.CompilerServices;

using RiceTea.Core;
using RiceTea.Core.Helpers;

using ShioUI.Graphics.Native.DirectWrite;
using ShioUI.Layout;
using ShioUI.Traits;
using ShioUI.Utils;

namespace ShioUI.Controls;

partial class TextBox : IAutoHeightElement
{
    public event MouseNotifyEventHandler? RequestContextMenu;
    public event KeyInteractEventHandler? KeyDown;
    public event KeyInteractEventHandler? KeyUp;
    public event TextChangingEventHandler? TextChanging;
    public event EventHandler? TextChanged;

    public SystemCursorType? Cursor => SystemCursorType.IBeam;

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

    public int CaretIndex
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Atomics.Read(ref _caretIndex);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            int oldCaretIndex = _caretIndex;
            if (Atomics.Exchange(ref oldCaretIndex, value) == value)
                return;
            int adjustedValue = AdjustCaretIndex(value, takeGreaterIfNotExists: false);
            if (Atomics.CompareExchange(ref oldCaretIndex, adjustedValue, value) != value)
                return;
            UpdateCaretIndex(adjustedValue);
        }
    }

    [AllowNull]
    public string Text
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Atomics.Read(ref _text);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => UpdateTextAndCaretIndex(value, _caretIndex);
    }

    [AllowNull]
    public string Watermark
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Atomics.Read(ref _watermark);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            value = FixString(value);
            if (ReferenceEquals(Atomics.Exchange(ref _watermark, value), value))
                return;

            Update(RenderObjectUpdateFlags.WatermarkLayout);
        }
    }

    public bool MultiLine
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _multiLine;
        set
        {
            if (_multiLine == value)
                return;
            _multiLine = value;

            string text = _text;
            if (value)
            {
                Size size = ContentSize;
                if (size.Width <= 0 || size.Height <= 0)
                    SurfaceSize = Size.Empty;
                else
                {
                    using DWriteTextLayout layout = CreateVirtualTextLayout(text);
                    layout.MaxWidth = size.Width;

                    SurfaceSize = new Size(0, MathI.Ceiling(layout.GetMetrics().Height) + UIConstants.ElementMargin);
                }
            }
            else
            {
                SurfaceSize = new Size(int.MaxValue, 0);
                Text = FixString(text);
            }
            Update(RenderObjectUpdateFlags.Format);
        }
    }

    public unsafe char PasswordChar
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            uint result = PasswordCodePoint;
            if (result > char.MaxValue)
                return '?';
            return (char)result;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => PasswordCodePoint = value;
    }

    public uint PasswordCodePoint
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Atomics.Read(ref _passwordCP);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            if (Atomics.Exchange(ref _passwordCP, value) == value)
                return;
            Update(RenderObjectUpdateFlags.Layout);
        }
    }

    public bool HasSelection => _selectionRange.Length > 0;

    public new LayoutNode AutoHeightDefinition
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _autoLayoutDefinitions[0] ??= new AutoHeightNode(this);
    }
}
