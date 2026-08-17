using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.CompilerServices;

using RiceTea.Core;
using RiceTea.Core.Extensions;

using ShioUI.Layout;
using ShioUI.Utils;

namespace ShioUI.Controls;

partial class GroupBox : IAutoWidthElement, IAutoHeightElement
{
    [AllowNull]
    public string Title
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Atomics.Read(ref _title);
        set
        {
            value ??= string.Empty;
            if (ReferenceEquals(Atomics.Exchange(ref _title, value), value))
                return;
            Update(RenderObjectUpdateFlags.Title, RedrawType.RedrawAllContent);
        }
    }

    [AllowNull]
    public string TitleDescription
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Atomics.Read(ref _titleDescription);
        set
        {
            value ??= string.Empty;
            if (ReferenceEquals(Atomics.Exchange(ref _titleDescription, value), value))
                return;
            Update(RenderObjectUpdateFlags.TitleDescription, RedrawType.RedrawAllContent);
        }
    }

    public GroupBoxMode Mode
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (GroupBoxMode)Atomics.Read(ref _mode);
        set
        {
            uint rawValue = (uint)value;
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(rawValue, (uint)GroupBoxMode._Last, nameof(value));
            if (Atomics.Exchange(ref _mode, rawValue) == rawValue)
                return;
            Update(RenderObjectUpdateFlags.Format, RedrawType.RedrawAllContent);
        }
    }

    public UIElement? FirstChild
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _children.FirstOrDefault();
    }

    public UIElement? LastChild
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _children.LastOrDefault();
    }

    public UIElementCollection Children
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _children;
    }

    public int ContentPageLeft
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetContentPageLeftCore();
    }

    public int ContentPageTop
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetContentPageTopCore();
    }

    public int ContentPageRight
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetContentPageRightCore(Width);
    }

    public int ContentPageBottom
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetContentPageBottomCore(Height);
    }

    public int ContentPageWidth
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetContentPageWidthCore(Width);
    }

    public int ContentPageHeight
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetContentPageHeightCore(Height);
    }

    public Point ContentPageOffset
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new Point(GetContentPageLeftCore(), GetContentPageTopCore());
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
