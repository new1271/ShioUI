using System.Drawing;
using System.Runtime.CompilerServices;

using RiceTea.Core.Extensions;

using ShioUI.Layout;

namespace ShioUI.Controls;

partial class GroupBox : IAutoWidthElement, IAutoHeightElement
{
    public string Title
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _title;
        set
        {
            if (ReferenceEquals(_title, value))
                return;
            _title = value ?? string.Empty;
            Update(RenderObjectUpdateFlags.Title, RedrawType.RedrawAllContent);
        }
    }

    public string Text
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _text;
        set
        {
            if (ReferenceEquals(_text, value))
                return;
            _text = value ?? string.Empty;
            Update(RenderObjectUpdateFlags.Text, RedrawType.RedrawText);
        }
    }

    public UIElement? FirstChild
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _children.GetUnderlyingList().FirstOrDefault();
    }

    public UIElement? LastChild
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _children.GetUnderlyingList().LastOrDefault();
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
