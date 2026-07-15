using System.Collections.Generic;

using RiceTea.Core;

using ShioUI.Graphics;
using ShioUI.Theme;
using ShioUI.Utils;
using ShioUI.Windows;

namespace ShioUI.Controls;

public sealed partial class PopupPanel : PopupElementBase, IElementContainer, ICheckableDisposable
{
    private readonly OneUIElementCollection _collection;

    public PopupPanel(CoreWindow window) : base(window, string.Empty)
    {
        _collection = new OneUIElementCollection(this);
    }

    protected override void ApplyThemeCore(IThemeResourceProvider provider) => UIElementHelper.ApplyThemeToElement(provider, _collection.Value);

    protected override bool RenderCore(in RegionalRenderingContext context) => true;

    public void RenderBackground(UIElement element, in RegionalRenderingContext context) => RenderBackground(context);

    IEnumerable<UIElement> IElementContainer.GetElements() => _collection;

    IEnumerable<UIElement> IElementContainer.GetActiveElements() => _collection;

    bool IElementContainer.IsBackgroundOpaque(UIElement element) => IsBackgroundOpaque();

    public ContentPageScope EnterContentPageScope() => ContentPageScope.Create(this);

    protected override void DisposeCore(bool disposing)
    {
        base.DisposeCore(disposing);
        if (disposing)
            _collection.Dispose();
    }
}
