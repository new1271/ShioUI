using System.Drawing;
using System.Runtime.CompilerServices;

using RiceTea.Core.Helpers;
using RiceTea.Core.Structures;

using ShioUI.Graphics;
using ShioUI.Graphics.Helpers;
using ShioUI.Graphics.Native.Direct2D.Brushes;
using ShioUI.Theme;
using ShioUI.Traits;
using ShioUI.Utils;

namespace ShioUI.Controls;

public sealed partial class ProgressBar : UIElement
{
    private static readonly string[] _brushNames = new string[(int)Brush._Last]
    {
        "back",
        "border",
        "fore"
    };

    private readonly D2D1Brush[] _brushes = new D2D1Brush[(int)Brush._Last];

    private double _value, _maximum;

    public ProgressBar(IElementContainer parent) : base(parent, "app.progressBar")
    {
        _value = 0.0f;
        _maximum = 100.0f;
    }

    protected override void ApplyThemeCore(IThemeResourceProvider provider)
        => UIElementHelper.ApplyThemeBrushesUnsafe(provider, _brushes, _brushNames, ThemePrefix, (nuint)Brush._Last);

    protected override bool IsBackgroundOpaqueCore() => GraphicsUtils.CheckBrushIsSolid(
        UnsafeHelper.AddTypedOffset(ref UnsafeHelper.GetArrayDataReference(_brushes), (nuint)Brush.BackBrush));

    protected override bool RenderCore(in RegionalRenderingContext context)
    {
        ref D2D1Brush brushesRef = ref UnsafeHelper.GetArrayDataReference(_brushes);
        SizeF renderSize = context.Size;

        double percentage, value = _value, maximum = _maximum;
        if (IsValidDouble(maximum))
        {
            if (IsValidDouble(value))
                percentage = MathHelper.Clamp(value / maximum, 0.0, 100.0);
            else if (double.IsPositiveInfinity(value))
                percentage = 100.0;
            else
                percentage = 0.0;
        }
        else if (double.IsPositiveInfinity(value))
            percentage = 0.0;
        else
            percentage = 100.0;

        RenderBackground(context, UnsafeHelper.AddTypedOffset(ref brushesRef, (nuint)Brush.BackBrush));
        context.FillRectangle(
            new RectF(0, 0, RenderingHelper.RoundInPixel((float)(renderSize.Width * percentage), context.PixelsPerPoint.X), renderSize.Height),
            UnsafeHelper.AddTypedOffset(ref brushesRef, (nuint)Brush.ForeBrush));
        context.DrawBorder(UnsafeHelper.AddTypedOffset(ref brushesRef, (nuint)Brush.BorderBrush));
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe bool IsValidDouble(double d)
    {
        ulong bits = *(ulong*)&d;
        return (bits & 0x7FF0000000000000UL) != 0x7FF0000000000000UL;
    }

    protected override void DisposeCore(bool disposing)
    {
        base.DisposeCore(disposing);
        if (disposing)
            DisposeHelper.DisposeAllUnsafe(in UnsafeHelper.GetArrayDataReference(_brushes), (nuint)Brush._Last);
        SequenceHelper.Clear(_brushes);
    }
}
