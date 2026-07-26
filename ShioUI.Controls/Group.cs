using System.Collections.Generic;
using System.Runtime.CompilerServices;

using RiceTea.Core;
using RiceTea.Core.Helpers;

using ShioUI.Graphics;
using ShioUI.Graphics.Native.Direct2D.Brushes;
using ShioUI.Theme;
using ShioUI.Utils;

namespace ShioUI.Controls;

public sealed partial class Group : UIElement, IAppendableElementContainer
{
    private static readonly string _backBrushName = "back";
    private readonly UIElementCollection _children;

    private D2D1Brush? _backBrush;
    private nuint _hasBackground;

    public Group(IElementContainer parent, string? themePrefix = null) : base(parent, themePrefix ?? string.Empty) => _children = new UIElementCollection(this);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddChild(UIElement element) => _children.Add(element);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddChildren(params UIElement[] elements) => _children.AddRange(elements);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddChildren(IEnumerable<UIElement> elements) => _children.AddRange(elements);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveChild(UIElement element) => _children.Remove(element);

    protected override void ApplyThemeCore(IThemeResourceProvider provider)
    {
        string themePrefix = ThemePrefix;
        if (StringHelper.IsNullOrEmpty(themePrefix))
            DisposeHelper.SwapDispose(ref _backBrush);
        else
            UIElementHelper.ApplyThemeBrush(provider, ref _backBrush, _backBrushName, themePrefix); 
        
        UIElementHelper.ApplyThemeToElements(provider, _children);
    }

    protected override bool RenderCore(in RegionalRenderingContext context)
    {
        if (Atomics.Read(ref _hasBackground) == 0)
            RenderBackground(context);
        else
            RenderBackground(context, _backBrush!);
        return true;
    }

    bool IElementContainer.IsBackgroundOpaque(UIElement element) => IsBackgroundOpaque();

    IEnumerable<UIElement?> IElementContainer.GetElements() => _children;

    IEnumerable<UIElement?> IElementContainer.GetActiveElements() => _children;

    void IElementContainer.RenderBackground(UIElement element, in RegionalRenderingContext context)
    {
        if (Atomics.Read(ref _hasBackground) == 0)
            RenderBackground(context);
        else
            RenderBackground(context, _backBrush!);
    }

    public ContentPageScope EnterContentPageScope() => ContentPageScope.Create(this);

    protected override void DisposeCore(bool disposing)
    {
        base.DisposeCore(disposing);
        if (disposing)
        {
            UIElementHelper.DisposeForElements(_children);
            _backBrush?.Dispose();
        }
        _children.Clear();
        _backBrush = null;
    }
}
