using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using ShioUI.Layout;

namespace ShioUI;

[StructLayout(LayoutKind.Auto)]
public readonly record struct ContentPageScopeParams(LayoutNode PageLeftDefinition, LayoutNode PageTopDefinition,
    LayoutNode PageRightDefinition, LayoutNode PageBottomDefinition, LayoutNode PageWidthDefinition, LayoutNode PageHeightDefinition)
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ContentPageScopeParams Create(LayoutNode leftDefinition, LayoutNode topDefinition,
        LayoutNode rightDefinition, LayoutNode bottomDefinition,
        LayoutNode widthDefinition, LayoutNode heightDefinition)
        => new ContentPageScopeParams(leftDefinition, topDefinition, rightDefinition, bottomDefinition, widthDefinition, heightDefinition);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ContentPageScopeParams Create<TContainerElement>(TContainerElement parent) where TContainerElement : UIElement, IElementContainer
    {
        LayoutNode emptyNode = LayoutNode.Empty;
        LayoutNode widthNode = parent.WidthDefinition;
        LayoutNode heightNode = parent.HeightDefinition;
        return new ContentPageScopeParams(emptyNode, emptyNode, widthNode, heightNode, widthNode, heightNode);
    }
}

[StructLayout(LayoutKind.Auto)]
public readonly ref struct ContentPageScope : IDisposable
{
    private readonly IElementContainer _owner;
    private readonly LayoutNode _leftDefinition, _topDefinition,
        _rightDefinition, _bottomDefinition, _widthDefinition, _heightDefinition;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ContentPageScope(IElementContainer owner,
        LayoutNode leftDefinition, LayoutNode topDefinition,
        LayoutNode rightDefinition, LayoutNode bottomDefinition,
        LayoutNode widthDefinition, LayoutNode heightDefinition)
    {
        _owner = owner;
        _leftDefinition = leftDefinition;
        _topDefinition = topDefinition;
        _rightDefinition = rightDefinition;
        _bottomDefinition = bottomDefinition;
        _widthDefinition = widthDefinition;
        _heightDefinition = heightDefinition;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ContentPageScope Create(IElementContainer parent,
        LayoutNode leftDefinition, LayoutNode topDefinition,
        LayoutNode rightDefinition, LayoutNode bottomDefinition,
        LayoutNode widthDefinition, LayoutNode heightDefinition)
        => new ContentPageScope(parent, leftDefinition, topDefinition, rightDefinition, bottomDefinition, widthDefinition, heightDefinition);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ContentPageScope Create(IElementContainer parent, in ContentPageScopeParams @params)
        => new ContentPageScope(parent, @params.PageLeftDefinition, @params.PageTopDefinition,
              @params.PageRightDefinition, @params.PageBottomDefinition,
              @params.PageWidthDefinition, @params.PageHeightDefinition);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ContentPageScope Create<TContainerElement>(TContainerElement parent) where TContainerElement : UIElement, IElementContainer
    {
        LayoutNode emptyNode = LayoutNode.Empty;
        LayoutNode widthNode = parent.WidthDefinition;
        LayoutNode heightNode = parent.HeightDefinition;
        return new ContentPageScope(parent, emptyNode, emptyNode, widthNode, heightNode, widthNode, heightNode);
    }

    public IElementContainer Owner
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _owner;
    }

    public LayoutNode PageLeftDefinition
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _leftDefinition;
    }

    public LayoutNode PageTopDefinition
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _topDefinition;
    }

    public LayoutNode PageRightDefinition
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _rightDefinition;
    }

    public LayoutNode PageBottomDefinition
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _bottomDefinition;
    }

    public LayoutNode PageWidthDefinition
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _leftDefinition;
    }

    public LayoutNode PageHeightDefinition
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _leftDefinition;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ContentPageScope WithDefaultMargin()
    {
        LayoutNode marginNode = UIConstants.ElementMarginDefinition,
            marginDoubleNode = UIConstants.ElementMarginDoubleDefinition;

        return new ContentPageScope(_owner,
            _leftDefinition + marginNode, _topDefinition + marginNode,
            _rightDefinition - marginNode, _bottomDefinition - marginNode,
            _widthDefinition - marginDoubleNode, _heightDefinition - marginDoubleNode);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ContentPageScope WithMargin(int margin)
        => WithMargin(margin, margin, margin, margin);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ContentPageScope WithMargin(int marginLeft, int marginTop, int marginRight, int marginBottom)
        => new ContentPageScope(_owner,
            _leftDefinition + LayoutNode.Fixed(marginLeft), _topDefinition + LayoutNode.Fixed(marginTop),
            _rightDefinition - LayoutNode.Fixed(marginRight), _bottomDefinition - LayoutNode.Fixed(marginBottom),
            _widthDefinition - LayoutNode.Fixed(marginLeft + marginRight), _heightDefinition - LayoutNode.Fixed(marginTop + marginBottom));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ContentPageScope WithMargin(LayoutNode margin)
        => WithMargin(margin, margin, margin, margin);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ContentPageScope WithMargin(LayoutNode marginLeft, LayoutNode marginTop, LayoutNode marginRight, LayoutNode marginBottom)
        => new ContentPageScope(_owner,
            _leftDefinition + marginLeft, _topDefinition + marginTop,
            _rightDefinition - marginRight, _bottomDefinition - marginBottom,
            _widthDefinition - (marginLeft + marginRight), _heightDefinition - (marginTop + marginBottom));

    public readonly void Dispose() { }
}
