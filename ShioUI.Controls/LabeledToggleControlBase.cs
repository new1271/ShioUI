using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Threading;

using InlineMethod;

using RiceTea.Core;
using RiceTea.Core.Extensions;
using RiceTea.Core.Helpers;

using ShioUI.Controls.Internals;
using ShioUI.Graphics;
using ShioUI.Graphics.Native.Direct2D.Brushes;
using ShioUI.Graphics.Native.DirectWrite;
using ShioUI.Layout;
using ShioUI.Theme;
using ShioUI.Traits;
using ShioUI.Utils;

namespace ShioUI.Controls;

public abstract partial class LabeledToggleControlBase : UIElement, IMouseInteractHandler, IMouseMoveHandler
{
    private readonly LayoutNode?[] _autoLayoutDefinitions = new LayoutNode?[2];
    private readonly bool _isRightSided;

    private string? _fontName;
    private string _text;
    private DWriteTextLayout? _layout;
    private WeakReference<LabeledToggleControlBase>? _reference;

    private ButtonTriState _buttonState;
    private long _rawUpdateFlags;
    private float _fontSize;
    private uint _state;
    private bool _isPressed;

    protected LabeledToggleControlBase(IElementContainer parent, string themePrefix) : base(parent, themePrefix)
    {
        _rawUpdateFlags = (long)RenderObjectUpdateFlags.FlagsAllTrue;
        _fontSize = UIConstants.DefaultFontSize;
        _state = default;
        _text = string.Empty;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private WeakReference<LabeledToggleControlBase> GetWeakReference()
    {
        WeakReference<LabeledToggleControlBase>? reference = Atomics.Read(ref _reference);
        if (reference is null)
        {
            reference = new WeakReference<LabeledToggleControlBase>(this);
            WeakReference<LabeledToggleControlBase>? oldReference = Atomics.CompareExchange(ref _reference, reference, null);
            if (oldReference is not null)
                reference = oldReference;
        }
        return reference;
    }

    protected override void ApplyThemeCore(IThemeResourceProvider provider)
    {
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

        RenderBackground(context);

        RectangleF toggleControlBounds, textBounds;
        toggleControlBounds = GetToggleControlBounds(context, renderSize.Height);
        if (_isRightSided)
            toggleControlBounds.X = renderSize.Width - toggleControlBounds.Width;
        RenderToggleControl(context, toggleControlBounds, State, buttonState);

        if (_isRightSided)
            textBounds = new RectangleF(0, 0, toggleControlBounds.X - UIConstants.ElementMarginHalf, renderSize.Height);
        else
            textBounds = RectangleF.FromLTRB(toggleControlBounds.Right + UIConstants.ElementMarginHalf, 0, renderSize.Width, renderSize.Height);

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
            context.DrawTextLayout(textBounds.Location, layout, GetTextBrush(buttonState));
        }
        finally
        {
            DisposeHelper.NullSwapOrDispose(ref _layout, layout);
        }

    Tail:
        return true;
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
        State = !State;
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

    protected abstract D2D1Brush GetTextBrush(ButtonTriState buttonState);

    protected abstract RectangleF GetToggleControlBounds(in RegionalRenderingContext context, float itemHeight);

    protected abstract void RenderToggleControl(in RegionalRenderingContext context, in RectangleF rect, bool toggled, ButtonTriState buttonState);

    protected abstract int CalculateToggleControlWidthForAutoSizing(int itemHeight);

    protected override void DisposeCore(bool disposing)
    {
        base.DisposeCore(disposing);
        if (disposing)
            DisposeHelper.SwapDisposeAtomic(ref _layout);
    }
}
