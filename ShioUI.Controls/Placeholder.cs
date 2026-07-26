using RiceTea.Core;
using RiceTea.Core.Helpers;

using ShioUI.Graphics;
using ShioUI.Graphics.Native.Direct2D.Brushes;
using ShioUI.Theme;
using ShioUI.Utils;

namespace ShioUI.Controls;

public sealed partial class Placeholder : UIElement
{
    private static readonly string _backBrushName = "back";

    private D2D1Brush? _backBrush;
    private nuint _hasBackground;

    public Placeholder(IElementContainer parent, string? themePrefix = null) : base(parent, themePrefix ?? string.Empty) { }

    protected override void ApplyThemeCore(IThemeResourceProvider provider)
    {
        string themePrefix = ThemePrefix;
        if (StringHelper.IsNullOrEmpty(themePrefix))
            DisposeHelper.SwapDispose(ref _backBrush);
        else
            UIElementHelper.ApplyThemeBrush(provider, ref _backBrush, _backBrushName, themePrefix);
    }

    protected override bool RenderCore(in RegionalRenderingContext context)
    {
        if (Atomics.Read(ref _hasBackground) == 0)
            RenderBackground(context);
        else
            RenderBackground(context, _backBrush!);
        return true;
    }

    protected override void DisposeCore(bool disposing)
    {
        base.DisposeCore(disposing);
        if (disposing)
            _backBrush?.Dispose();
        _backBrush = null;
    }
}
