using System.Drawing;

using RiceTea.Core.Extensions;
using RiceTea.Core.Helpers;
using RiceTea.Core.Structures;

using ShioUI.Graphics;
using ShioUI.Graphics.Native.Direct2D;
using ShioUI.Graphics.Native.Direct2D.Brushes;
using ShioUI.Theme;
using ShioUI.Traits;
using ShioUI.Utils;

namespace ShioUI.Controls;

public sealed partial class ToggleSwitch : LabeledToggleControlBase
{
    private static readonly string[] _brushNames = new string[(int)Brush._Last]
    {
        "border",
        "border.hovered" ,
        "border.pressed",
        "thumb",
        "thumb.hovered",
        "thumb.pressed",
        "track",
        "track.checked",
        "fore",
        "fore.hovered",
        "fore.pressed"
    };

    private readonly D2D1Brush[] _brushes = new D2D1Brush[(int)Brush._Last];

    public ToggleSwitch(IElementContainer parent) : base(parent, "app.toggleSwitch") { }

    protected override void ApplyThemeCore(IThemeResourceProvider provider)
    {
        UIElementHelper.ApplyThemeBrushesUnsafe(provider, _brushes, _brushNames, ThemePrefix, (nuint)Brush._Last);
        base.ApplyThemeCore(provider);
    }

    protected override int CalculateToggleControlWidthForAutoSizing(int itemHeight) => itemHeight * 2;

    protected override D2D1Brush GetTextBrush(ButtonTriState buttonState) => _brushes.AsUnsafeRef()[(nuint)Brush.ForeBrush + (nuint)buttonState];

    protected override RectangleF GetToggleControlBounds(in RegionalRenderingContext context, float itemHeight)
        => GetToggleSwitchRenderingBounds(in context, itemHeight);

    protected override void RenderToggleControl(in RegionalRenderingContext context, in RectangleF rect, bool toggled, ButtonTriState buttonState)
        => DrawToggleSwitchCore(context, in UnsafeHelper.GetArrayDataReference(_brushes), rect, toggled, buttonState);

    public static RectangleF GetToggleSwitchRenderingBounds(in RegionalRenderingContext context, float itemHeight)
    {
        float borderWidth = context.DefaultBorderWidth;
        float buttonWidth = itemHeight * 2 - borderWidth * 2;
        float buttonHeight = itemHeight - borderWidth * 2;
        return context.GetPixelAlignedRect(new RectangleF(borderWidth, borderWidth, buttonWidth, buttonHeight));
    }

    public static void DrawToggleSwitch(in RegionalRenderingContext context, D2D1Brush[] brushes, in RectangleF renderingBounds,
        bool toggleState, ButtonTriState buttonState)
    {
        if (buttonState > ButtonTriState.Pressed || brushes.Length < (int)Brush._ToggleSwitchRenderingLast)
            return;
        DrawToggleSwitchCore(context, in UnsafeHelper.GetArrayDataReference(brushes), renderingBounds, toggleState, buttonState);
    }

    private static void DrawToggleSwitchCore(in RegionalRenderingContext context, ref readonly D2D1Brush? brushesRef, in RectangleF renderingBounds,
        bool toggleState, ButtonTriState buttonState)
    {
        D2D1Brush? borderBrush = UnsafeHelper.AddTypedOffsetAsReadOnly(in brushesRef, (nuint)Brush.BorderBrush + (nuint)buttonState);
        D2D1Brush? buttonBrush = UnsafeHelper.AddTypedOffsetAsReadOnly(in brushesRef, (nuint)Brush.ToggleSwitchButtonBrush + (nuint)buttonState);
        D2D1Brush? railBrush = UnsafeHelper.AddTypedOffsetAsReadOnly(in brushesRef, (nuint)Brush.ToggleSwitchRailBrush + MathHelper.BooleanToNativeUnsigned(toggleState));
        if (borderBrush is null || buttonBrush is null || railBrush is null)
            return;

        using RegionalRenderingContext clipContext = context.WithPixelAlignedClip(renderingBounds, D2D1AntialiasMode.Aliased, out RectF _);

        RectangleF railRect = new RectangleF(UIConstants.ElementMarginHalf, UIConstants.ElementMarginHalf,
            renderingBounds.Width - UIConstants.ElementMargin, renderingBounds.Height - UIConstants.ElementMargin);

        SizeF buttonSize = new SizeF(renderingBounds.Width / 4, renderingBounds.Height);

        PointF buttonLocation;

        if (toggleState)
        {
            float buttonX = renderingBounds.Width - buttonSize.Width;
            buttonLocation = new PointF(buttonX, 0);
        }
        else
        {
            buttonLocation = PointF.Empty;
        }

        clipContext.FillRectangle(railRect, railBrush);
        clipContext.DrawBorder(borderBrush);
        D2D1DeviceContext deviceContext = clipContext.DeviceContext;
        (D2D1AntialiasMode lastAntialiasMode, deviceContext.AntialiasMode) = (deviceContext.AntialiasMode, D2D1AntialiasMode.PerPrimitive);
        try
        {
            clipContext.FillRectangle(new RectangleF(buttonLocation, buttonSize), buttonBrush);
        }
        finally
        {
            clipContext.DeviceContext.AntialiasMode = lastAntialiasMode;
        }
    }

    protected override void DisposeCore(bool disposing)
    {
        base.DisposeCore(disposing);
        if (disposing)
            DisposeHelper.DisposeAllUnsafe(in UnsafeHelper.GetArrayDataReference(_brushes), (nuint)Brush._Last);
        SequenceHelper.Clear(_brushes);
    }
}
