using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using ShioUI.Controls.Internals;
using ShioUI.Utils;
using ShioUI.Windows;
using ShioUI.Graphics;
using ShioUI.Graphics.Native.Direct2D.Brushes;
using ShioUI.Theme;

using RiceTea.Core.Collections;
using RiceTea.Core.Helpers;

namespace ShioUI.Controls;

public sealed partial class PopupContainer : PopupElementBase, IAppendableElementContainer
{
    private static readonly string[] _brushNames = new string[(int)Brush._Last]
    {
        "back",
        "border"
    };

    private readonly D2D1Brush[] _brushes = new D2D1Brush[(int)Brush._Last];
    private readonly UIElementCollection _children;

    public PopupContainer(CoreWindow window) : base(window, "app.control")
    {
        _children = new UIElementCollection(this);
    }

    protected override void ApplyThemeCore(IThemeResourceProvider provider)
    {
        UIElementHelper.ApplyThemeBrushesUnsafe(provider, _brushes, _brushNames, ThemePrefix, (nuint)Brush._Last);
        UIElementHelper.ApplyThemeToElements(provider, _children);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddChild(UIElement element) => _children.Add(element);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddChildren(params UIElement[] elements) => _children.AddRange(elements);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddChildren(IEnumerable<UIElement> elements) => _children.AddRange(elements);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveChild(UIElement element) => _children.Remove(element);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RenderBackground(UIElement element, in RegionalRenderingContext context)
        => RenderBackground(context, UnsafeHelper.AddTypedOffset(ref UnsafeHelper.GetArrayDataReference(_brushes), (nuint)Brush.BackBrush));

    protected override bool IsBackgroundOpaqueCore() => GraphicsUtils.CheckBrushIsSolid(_brushes[(int)Brush.BackBrush]);

    bool IElementContainer.IsBackgroundOpaque(UIElement element) => IsBackgroundOpaque();

    public ContentPageScope EnterContentPageScope() => ContentPageScope.Create(this);

    protected override bool RenderCore(in RegionalRenderingContext context)
    {
        ref D2D1Brush brushesRef = ref UnsafeHelper.GetArrayDataReference(_brushes);
        RenderBackground(context, UnsafeHelper.AddTypedOffset(ref brushesRef, (nuint)Brush.BackBrush));
        context.DrawBorder(UnsafeHelper.AddTypedOffset(ref brushesRef, (nuint)Brush.BorderBrush));

        return true;
    }

    IEnumerable<UIElement?> IElementContainer.GetElements() => _children;

    IEnumerable<UIElement?> IElementContainer.GetActiveElements() => _children;

    protected override void DisposeCore(bool disposing)
    {
        base.DisposeCore(disposing);
        if (disposing)
        {
            DisposeHelper.DisposeAllUnsafe(in UnsafeHelper.GetArrayDataReference(_brushes), (nuint)Brush._Last);
            _children.Dispose();
        }
        SequenceHelper.Clear(_brushes);
    }
}
