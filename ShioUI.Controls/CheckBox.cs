using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;

using InlineMethod;

using RiceTea.Core;
using RiceTea.Core.Extensions;
using RiceTea.Core.Helpers;
using RiceTea.Core.Structures;

using ShioUI.Controls.Internals;
using ShioUI.Graphics;
using ShioUI.Graphics.Helpers;
using ShioUI.Graphics.Native.Direct2D;
using ShioUI.Graphics.Native.Direct2D.Brushes;
using ShioUI.Graphics.Native.DirectWrite;
using ShioUI.Layout;
using ShioUI.Theme;
using ShioUI.Traits;
using ShioUI.Utils;

namespace ShioUI.Controls;

public sealed partial class CheckBox : UIElement, IMouseInteractHandler, IMouseMoveHandler
{
    private static readonly string[] _brushNames = new string[(int)Brush._Last]
    {
        "border",
        "border.hovered" ,
        "border.pressed",
        "border.checked" ,
        "border.hovered.checked" ,
        "border.pressed.checked",
        "mark",
        "switch.thumb",
        "switch.thumb.hovered",
        "switch.thumb.pressed",
        "switch.track",
        "switch.track.checked",
        "fore",
        "fore.hovered",
        "fore.pressed"
    };

    private readonly D2D1Brush[] _brushes = new D2D1Brush[(int)Brush._Last];
    private readonly LayoutNode?[] _autoLayoutDefinitions = new LayoutNode?[2];
    private readonly CheckBoxMode _mode;
    private readonly bool _isRightSided;

    private string? _fontName;
    private string _text;
    private DWriteTextLayout? _layout;
    private WeakReference<CheckBox>? _reference;

    private ButtonTriState _buttonState;
    private long _rawUpdateFlags;
    private float _fontSize;
    private uint _checked;
    private bool _isPressed;

    public CheckBox(IElementContainer parent) : base(parent, "app.checkBox")
    {
        _rawUpdateFlags = (long)RenderObjectUpdateFlags.FlagsAllTrue;
        _fontSize = UIConstants.DefaultFontSize;
        _checked = default;
        _text = string.Empty;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private WeakReference<CheckBox> GetWeakReference()
    {
        WeakReference<CheckBox>? reference = Atomics.Read(ref _reference);
        if (reference is null)
        {
            reference = new WeakReference<CheckBox>(this);
            WeakReference<CheckBox>? oldReference = Atomics.CompareExchange(ref _reference, reference, null);
            if (oldReference is not null)
                reference = oldReference;
        }
        return reference;
    }

    protected override void ApplyThemeCore(IThemeResourceProvider provider)
    {
        UIElementHelper.ApplyThemeBrushesUnsafe(provider, _brushes, _brushNames, ThemePrefix, (nuint)Brush._Last);
        _fontName = provider.FontName;
        _rawUpdateFlags = -1L;
        _fontSize = UIConstants.DefaultFontSize;
        DisposeHelper.SwapDispose(ref _layout);
    }

    public override void OnSizeChanged() => Update();

    [Inline(InlineBehavior.Remove)]
    private RenderObjectUpdateFlags GetAndCleanRenderObjectUpdateFlags()
        => (RenderObjectUpdateFlags)Interlocked.Exchange(ref _rawUpdateFlags, default);

    private DWriteTextLayout? GetTextLayout(RenderObjectUpdateFlags flags)
    {
        DWriteTextLayout? layout = Atomics.Exchange(ref _layout, null);

        if ((flags & RenderObjectUpdateFlags.Layout) == RenderObjectUpdateFlags.Layout)
        {
            DWriteTextFormat? format = layout;
            if (CheckFormatIsNotAvailable(format, flags))
                format = TextFormatHelper.CreateTextFormat(TextAlignment.MiddleLeft, NullSafetyHelper.ThrowIfNull(_fontName), _fontSize);
            string text = _text;
            if (StringHelper.IsNullOrEmpty(text))
                layout = null;
            else
                layout = SharedResources.DWriteFactory.CreateTextLayout(text, format);
            format.Dispose();
        }
        return layout;
    }

    [Inline(InlineBehavior.Remove)]
    private static bool CheckFormatIsNotAvailable([NotNullWhen(false)] DWriteTextFormat? format, RenderObjectUpdateFlags flags)
    {
        if (format is null || format.IsDisposed)
            return true;
        if ((flags & RenderObjectUpdateFlags.Format) == RenderObjectUpdateFlags.Format)
        {
            format.Dispose();
            return true;
        }
        return false;
    }

    protected override bool RenderCore(in RegionalRenderingContext context)
    {
        SizeF renderSize = context.Size;
        RenderObjectUpdateFlags flags = GetAndCleanRenderObjectUpdateFlags();
        ButtonTriState buttonState = _buttonState;
        if (buttonState >= ButtonTriState.Pressed)
            buttonState = ButtonTriState.None;

        ref readonly D2D1Brush brushesRef = ref UnsafeHelper.GetArrayDataReference(_brushes);
        D2D1Brush textBrush = UnsafeHelper.AddTypedOffsetAsReadOnly(in brushesRef, (nuint)Brush.ForeBrush + (uint)buttonState);
        RenderBackground(context);

        RectangleF checkBoxBounds, textBounds;
        switch (_mode)
        {
            case CheckBoxMode.Regular:
                {
                    checkBoxBounds = GetCheckBoxRenderingBounds(in context, renderSize.Height);
                    if (_isRightSided)
                        checkBoxBounds.X = renderSize.Width - checkBoxBounds.Width;
                    DrawCheckBoxCore(context, in brushesRef, checkBoxBounds, Checked, buttonState);
                }
                break;
            case CheckBoxMode.ToggleSwitch:
                {
                    checkBoxBounds = GetToggleSwitchRenderingBounds(in context, renderSize.Height);
                    if (_isRightSided)
                        checkBoxBounds.X = renderSize.Width - checkBoxBounds.Width;
                    DrawToggleSwitchCore(context, in brushesRef, checkBoxBounds, Checked, buttonState);
                }
                break;
            default:
                {
                    if (_isRightSided)
                        checkBoxBounds = new RectangleF(renderSize.Width, 0, 0, 0);
                    else
                        checkBoxBounds = default;
                }
                break;
        }
        if (_isRightSided)
            textBounds = new RectangleF(0, 0, checkBoxBounds.X - UIConstants.ElementMarginHalf, renderSize.Height);
        else
            textBounds = RectangleF.FromLTRB(checkBoxBounds.Right + UIConstants.ElementMarginHalf, 0, renderSize.Width, renderSize.Height);

        DWriteTextLayout? layout = GetTextLayout(flags);
        if (layout is null)
            goto Tail;
        try
        {
            float width = textBounds.Width;
            float height = textBounds.Height;
            if (width <= 0.0f || height <= 0.0f)
                goto Tail;

            layout.MaxWidth = textBounds.Width;
            layout.MaxHeight = textBounds.Height;
            context.DrawTextLayout(textBounds.Location, layout, textBrush);
        }
        finally
        {
            DisposeHelper.NullSwapOrDispose(ref _layout, layout);
        }

    Tail:
        return true;
    }

    private void DrawCheckBox(in RegionalRenderingContext context, ref readonly D2D1Brush? brushesRef, ButtonTriState buttonState)
        => DrawCheckBoxCore(in context, in brushesRef, GetCheckBoxRenderingBounds(in context, context.Size.Height), Checked, buttonState);

    public static RectangleF GetCheckBoxRenderingBounds(in RegionalRenderingContext context, float itemHeight)
    {
        float borderWidth = context.DefaultBorderWidth;
        float buttonWidth = itemHeight - borderWidth * 2;
        return context.GetPixelAlignedRect(new RectangleF(borderWidth, borderWidth, buttonWidth, buttonWidth));
    }

    public static RectangleF GetToggleSwitchRenderingBounds(in RegionalRenderingContext context, float itemHeight)
    {
        float borderWidth = context.DefaultBorderWidth;
        float buttonWidth = itemHeight * 2 - borderWidth * 2;
        float buttonHeight = itemHeight - borderWidth * 2;
        return context.GetPixelAlignedRect(new RectangleF(borderWidth, borderWidth, buttonWidth, buttonHeight));
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

    private static void DrawToggleSwitchCore(in RegionalRenderingContext context, ref readonly D2D1Brush? brushesRef, in RectangleF renderingBounds,
        bool checkState, ButtonTriState buttonState)
    {
        D2D1Brush? borderBrush = UnsafeHelper.AddTypedOffsetAsReadOnly(in brushesRef, (nuint)Brush.BorderBrush + (nuint)buttonState);
        D2D1Brush? buttonBrush = UnsafeHelper.AddTypedOffsetAsReadOnly(in brushesRef, (nuint)Brush.ToggleSwitchButtonBrush + (nuint)buttonState);
        D2D1Brush? railBrush = UnsafeHelper.AddTypedOffsetAsReadOnly(in brushesRef, (nuint)Brush.ToggleSwitchRailBrush + MathHelper.BooleanToNativeUnsigned(checkState));
        if (borderBrush is null || buttonBrush is null || railBrush is null)
            return;

        using RegionalRenderingContext clipContext = context.WithPixelAlignedClip(renderingBounds, D2D1AntialiasMode.Aliased, out RectF _);

        RectangleF railRect = new RectangleF(UIConstants.ElementMarginHalf, UIConstants.ElementMarginHalf,
            renderingBounds.Width - UIConstants.ElementMargin, renderingBounds.Height - UIConstants.ElementMargin);

        SizeF buttonSize = new SizeF(renderingBounds.Width / 4, renderingBounds.Height);

        PointF buttonLocation;

        if (checkState)
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

    public void OnMouseMove(in MouseEventArgs args)
    {
        ButtonTriState oldButtonState = _buttonState;
        ButtonTriState newButtonState;
        if (args.IsInSpecificSize(Size))
            newButtonState = _isPressed ? ButtonTriState.Pressed : ButtonTriState.Hovered;
        else
            newButtonState = ButtonTriState.None;
        if (oldButtonState == newButtonState)
            return;
        _buttonState = newButtonState;
        Update();
    }

    public void OnMouseDown(ref HandleableMouseEventArgs args)
    {
        if (_buttonState != ButtonTriState.Hovered || !args.Buttons.HasFlagFast(MouseButtons.LeftButton))
            return;

        args.Handle();
        _isPressed = true;
        _buttonState = ButtonTriState.Pressed;
        Checked = !Checked;
    }

    public void OnMouseUp(in MouseEventArgs args)
    {
        if (!args.Buttons.HasFlagFast(MouseButtons.LeftButton))
            return;
        _isPressed = false;

        if (_buttonState != ButtonTriState.Pressed)
            return;
        _buttonState = ButtonTriState.Hovered;
        Update();
    }

    protected override void DisposeCore(bool disposing)
    {
        base.DisposeCore(disposing);
        if (disposing)
        {
            DisposeHelper.SwapDisposeAtomic(ref _layout);
            DisposeHelper.DisposeAllUnsafe(in UnsafeHelper.GetArrayDataReference(_brushes), (nuint)Brush._Last);
        }
        SequenceHelper.Clear(_brushes);
    }
}
