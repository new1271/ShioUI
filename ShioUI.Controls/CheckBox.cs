using System.Drawing;

using RiceTea.Core.Extensions;
using RiceTea.Core.Helpers;
using RiceTea.Core.Structures;

using ShioUI.Controls.Internals;
using ShioUI.Graphics;
using ShioUI.Graphics.Native.Direct2D;
using ShioUI.Graphics.Native.Direct2D.Brushes;
using ShioUI.Theme;
using ShioUI.Traits;
using ShioUI.Utils;

namespace ShioUI.Controls;

public sealed partial class CheckBox : LabeledToggleControlBase
{
    private static readonly string[] _brushNames = new string[(int)Brush._Last]
    {
        "border",
        "border.hovered" ,
        "border.pressed",
        "border.checked",
        "border.hovered.checked" ,
        "border.pressed.checked",
        "mark",
        "fore",
        "fore.hovered",
        "fore.pressed"
    };

    private readonly D2D1Brush[] _brushes = new D2D1Brush[(int)Brush._Last];

    public CheckBox(IElementContainer parent) : base(parent, "app.checkBox") { }

    protected override void ApplyThemeCore(IThemeResourceProvider provider)
    {
        UIElementHelper.ApplyThemeBrushesUnsafe(provider, _brushes, _brushNames, ThemePrefix, (nuint)Brush._Last);
        base.ApplyThemeCore(provider);
    }

    protected override int CalculateToggleControlWidthForAutoSizing(int itemHeight) => itemHeight;

    protected override D2D1Brush GetTextBrush(ButtonTriState buttonState) => _brushes.AsUnsafeRef()[(nuint)Brush.ForeBrush + (nuint)buttonState];

    protected override RectangleF GetToggleControlBounds(in RegionalRenderingContext context, float itemHeight)
        => GetCheckBoxRenderingBounds(in context, itemHeight);

    protected override void RenderToggleControl(in RegionalRenderingContext context, in RectangleF rect, bool toggled, ButtonTriState buttonState)
        => DrawCheckBoxCore(in context, in UnsafeHelper.GetArrayDataReference(_brushes), GetCheckBoxRenderingBounds(in context, context.Size.Height), State, buttonState);

    public static RectangleF GetCheckBoxRenderingBounds(in RegionalRenderingContext context, float itemHeight)
    {
        float borderWidth = context.DefaultBorderWidth;
        float buttonWidth = itemHeight - borderWidth * 2;
        return context.GetPixelAlignedRect(new RectangleF(borderWidth, borderWidth, buttonWidth, buttonWidth));
    }

    public static void DrawCheckBox(in RegionalRenderingContext context, D2D1Brush?[] brushes, in RectangleF renderingBounds,
        bool checkState, ButtonTriState buttonState)
    {
        if (buttonState > ButtonTriState.Pressed || brushes.Length < (int)Brush._CheckBoxRenderingLast)
            return;
        DrawCheckBoxCore(context, ref brushes.AsUnsafeRef().FirstElement, renderingBounds, checkState, buttonState);
    }

    private static void DrawCheckBoxCore(in RegionalRenderingContext context, ref readonly D2D1Brush? brushesRef, in RectangleF renderingBounds,
        bool checkState, ButtonTriState buttonState)
    {
        D2D1Brush? borderBrush = UnsafeHelper.AddTypedOffsetAsReadOnly(in brushesRef,
            MathHelper.BooleanToNativeUnsigned(checkState) * (nuint)Brush.BorderCheckedBrush + (nuint)buttonState);
        if (borderBrush is null)
            return;

        using RegionalRenderingContext clipContext = context.WithPixelAlignedClip(renderingBounds, D2D1AntialiasMode.Aliased, out RectF _);
        if (checkState)
        {
            RectF bounds = clipContext.Bounds;
            clipContext.FillRectangle(bounds, borderBrush);
            D2D1Brush? markBrush = UnsafeHelper.AddTypedOffsetAsReadOnly(in brushesRef, (nuint)Brush.MarkBrush);
            if (markBrush is null)
                return;
            FontIconResources.Instance.DrawCheckMark(clipContext, new RectangleF(PointF.Empty, clipContext.Size), markBrush);
        }
        else
        {
            clipContext.DrawBorder(borderBrush);
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
