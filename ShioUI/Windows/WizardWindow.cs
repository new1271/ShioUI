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

using ShioUI.Graphics;
using ShioUI.Graphics.Helpers;
using ShioUI.Graphics.Native.Direct2D;
using ShioUI.Graphics.Native.Direct2D.Brushes;
using ShioUI.Graphics.Native.DirectWrite;
using ShioUI.Internals;
using ShioUI.Theme;
using ShioUI.Utils;

namespace ShioUI.Windows;

public abstract class WizardWindow : MultiPageWindow
{
    #region Enums
    [Flags]
    private enum UpdateFlags : long
    {
        None = 0,
        UpdateCaption = 0b01,
        UpdateCaptionDescription = 0b10,
        All = UpdateCaption | UpdateCaptionDescription,
    }

    protected new enum Brush
    {
        WizardTitleBrush,
        WizardTitleDescriptionBrush,
        _Last
    }
    #endregion

    #region Static Fields
    private static readonly string[] _brushNames = new string[(int)Brush._Last]
    {
        "app.control.fore",
        "app.control.fore.description"
    }.ToLowerAscii();
    #endregion

    #region Fields
    private readonly D2D1Brush[] _brushes = new D2D1Brush[(int)Brush._Last];
    private DWriteTextLayout? _titleLayout, _titleDescriptionLayout;
    private string _heading = string.Empty, _headingDescription = string.Empty;
    private long _updateFlags = -1L;
    private D2D1ColorF _wizardBaseColor;
    private Point _titleLocation, _titleDescriptionLocation;
    private Rectangle _widePageBounds;
    #endregion

    #region Constructor
    protected WizardWindow() : base() => Initialize();

    protected WizardWindow(GraphicsDeviceProvider? deviceProvider) : base(deviceProvider) => Initialize();

    protected WizardWindow(CoreWindow parent, bool passParentToUnderlyingWindow = true) : base(parent, passParentToUnderlyingWindow) => Initialize();
    #endregion

    #region Properties

    [AllowNull]
    public string Heading
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Atomics.Read(ref _heading);
        set
        {
            Atomics.Write(ref _heading, value ?? string.Empty);
            Atomics.Or(ref _updateFlags, (long)UpdateFlags.UpdateCaption);
            UpdateAndResize();
        }
    }

    [AllowNull]
    public string HeadingDescription
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Atomics.Read(ref _headingDescription);
        protected set
        {
            Atomics.Write(ref _headingDescription, value ?? string.Empty);
            Atomics.Or(ref _updateFlags, (long)UpdateFlags.UpdateCaptionDescription);
            UpdateAndResize();
        }
    }
    #endregion

    #region Override Methods

    protected override CreateWindowInfo GetCreateWindowInfo()
    {
        CreateWindowInfo windowInfo = base.GetCreateWindowInfo();
        windowInfo.Styles = (windowInfo.Styles & WindowStyles.SystemMenu) | WindowStyles.DialogFrame;
        return windowInfo;
    }

    protected override void ApplyThemeCore(IThemeResourceProvider provider)
    {
        base.ApplyThemeCore(provider);
        UIElementHelper.ApplyThemeBrushesUnsafe(provider, _brushes, _brushNames, (nuint)Brush._Last);
        _wizardBaseColor = provider.TryGetColor(ThemeConstants.WizardWindowBaseColor, out D2D1ColorF color) ? color : default;
        DisposeHelper.SwapDisposeAtomic(ref _titleLayout, null);
        DisposeHelper.SwapDisposeAtomic(ref _titleDescriptionLayout, null);
        Interlocked.Exchange(ref _updateFlags, -1L);
    }

    public override void RenderBackground(UIElement element, in RegionalRenderingContext context) => ClearDC(context);

    protected override void RenderPageBackground(in RegionalRenderingContext context, in WindowRenderingData data) => ClearDC(context);

    protected override bool IsBackgroundOpaque() => _wizardBaseColor.A >= 1.0f;

    private void GetLayouts(UpdateFlags flags, out DWriteTextLayout? titleLayout, out DWriteTextLayout? titleDescriptionLayout)
    {
        titleLayout = Interlocked.Exchange(ref _titleLayout, null);
        titleDescriptionLayout = Interlocked.Exchange(ref _titleDescriptionLayout, null);
        if ((flags & UpdateFlags.UpdateCaption) == UpdateFlags.UpdateCaption)
        {
            DWriteFactory factory = SharedResources.DWriteFactory;
            DWriteTextFormat? format = titleLayout;
            if (format is null)
            {
                string fontName = NullSafetyHelper.ThrowIfNull(CurrentTheme).FontName;
                format = factory.CreateTextFormat(fontName, UIConstants.WizardWindowTitleFontSize);
                format.WordWrapping = DWriteWordWrapping.Wrap;
            }
            titleLayout = factory.CreateTextLayout(_heading, format);
            format.Dispose();
        }
        if ((flags & UpdateFlags.UpdateCaptionDescription) == UpdateFlags.UpdateCaptionDescription)
        {
            DWriteFactory factory = SharedResources.DWriteFactory;
            DWriteTextFormat? format = titleDescriptionLayout;
            if (format is null)
            {
                string fontName = NullSafetyHelper.ThrowIfNull(CurrentTheme).FontName;
                format = factory.CreateTextFormat(fontName, UIConstants.WizardWindowTitleDescriptionFontSize);
                format.WordWrapping = DWriteWordWrapping.Wrap;
            }
            titleDescriptionLayout = factory.CreateTextLayout(_headingDescription, format);
            format.Dispose();
        }
    }

    protected override void RenderTitle(D2D1DeviceContext deviceContext, DirtyAreaCollector collector, bool force, in WindowRenderingData data)
    {
        base.RenderTitle(deviceContext, collector, force, in data);
        UpdateFlags flags = (UpdateFlags)Interlocked.Exchange(ref _updateFlags, 0L);
        if (flags == 0L && !force)
            return;
        GetLayouts(flags, out DWriteTextLayout? titleLayout, out DWriteTextLayout? titleDescriptionLayout);
        ref D2D1Brush brushesRef = ref UnsafeHelper.GetArrayDataReference(_brushes);
        Rect rect = _widePageBounds;
        if (ActualWindowMaterial == WindowMaterial.Integrated)
            rect = new Rect(0, 0, ClientSize.Width, rect.Top);
        else
            rect = new Rect(rect.Left, data.Layout.TitleBarBounds.Bottom, rect.Right, rect.Top);
        Vector2 dpiScaleFactor = DpiScaleFactor;
        using (RenderingClipScope scope = RenderingClipScope.Enter(deviceContext, RenderingHelper.RoundInPixel(rect, dpiScaleFactor)))
        {
            ClearDC(deviceContext);
            if (titleLayout is not null)
            {
                deviceContext.DrawTextLayout(_titleLocation, titleLayout,
                    UnsafeHelper.AddTypedOffset(ref brushesRef, (nuint)Brush.WizardTitleBrush), D2D1DrawTextOptions.None);
                DisposeHelper.NullSwapOrDispose(ref _titleLayout, titleLayout);
            }
            if (titleDescriptionLayout is not null)
            {
                deviceContext.DrawTextLayout(_titleDescriptionLocation, titleDescriptionLayout,
                    UnsafeHelper.AddTypedOffset(ref brushesRef, (nuint)Brush.WizardTitleDescriptionBrush), D2D1DrawTextOptions.None);
                DisposeHelper.NullSwapOrDispose(ref _titleDescriptionLayout, titleDescriptionLayout);
            }
            collector.MarkAsDirty(scope.ClipRect);
        }

        if (force)
        {
            using RenderingClipScope scope = RenderingClipScope.Enter(deviceContext, RenderingHelper.RoundInPixel(_widePageBounds, dpiScaleFactor), D2D1AntialiasMode.Aliased);
            ClearDC(deviceContext);
        }
    }

    protected override void RecalculateLayout(ref WindowLayoutData data, Size windowSize)
    {
        base.RecalculateLayout(ref data, windowSize);
        Rectangle pageBounds = data.PageBounds;
        Rectangle widePageBounds = pageBounds;
        Rect pageRect = new Rect(
            pageBounds.X + UIConstantsPrivate.WizardLeftPadding,
            pageBounds.Y + UIConstantsPrivate.WizardPadding,
            pageBounds.Right - UIConstantsPrivate.WizardPadding,
            pageBounds.Bottom - UIConstantsPrivate.WizardPadding);
        _titleLocation = pageRect.Location;
        GetLayouts((UpdateFlags)Interlocked.Exchange(ref _updateFlags, 0L), out DWriteTextLayout? titleLayout, out DWriteTextLayout? titleDescriptionLayout);
        if (titleLayout is not null)
        {
            titleLayout.MaxWidth = pageRect.Width;
            int descriptionLocY = MathI.Ceiling(pageRect.Y + titleLayout.GetMetrics().Height + UIConstantsPrivate.WizardSubtitleMargin);
            if (titleDescriptionLayout is null)
                pageRect.Y = descriptionLocY;
            else
            {
                _titleDescriptionLocation = new Point(pageRect.X + UIConstantsPrivate.WizardSubtitleLeftMargin, descriptionLocY);
                titleDescriptionLayout.MaxWidth = pageRect.Width - UIConstantsPrivate.WizardSubtitleLeftMargin;
                pageRect.Y = descriptionLocY + MathI.Ceiling(titleDescriptionLayout.GetMetrics().Height);
                DisposeHelper.NullSwapOrDispose(ref _titleDescriptionLayout, titleDescriptionLayout);
            }
            DisposeHelper.NullSwapOrDispose(ref _titleLayout, titleLayout);
        }
        pageBounds = (Rectangle)pageRect;

        widePageBounds.Y = pageBounds.Y;
        widePageBounds.Height = pageBounds.Height;
        _widePageBounds = widePageBounds;
        data.PageBounds = pageBounds;
    }

    protected override HitTestValue CustomHitTest(PointF clientPoint)
    {
        HitTestValue result = base.CustomHitTest(clientPoint);
        if (result == HitTestValue.NoWhere)
            return PageBounds.Contains(clientPoint) ? HitTestValue.Client : HitTestValue.Caption;
        return result;
    }
    #endregion

    #region Inline Methods
    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ClearDC(D2D1DeviceContext context) => context.Clear(_wizardBaseColor);

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ClearDC(in RegionalRenderingContext context) => context.Clear(_wizardBaseColor);

    protected override void ClearDCForTitle(D2D1DeviceContext deviceContext)
    {
        if (ActualWindowMaterial == WindowMaterial.Integrated)
            ClearDC(deviceContext);
        else
            deviceContext.Clear(ClearDCColor);
    }
    #endregion

    private void Initialize()
    {
        MinimizeBox = false;
        MaximizeBox = false;
        ShowTitle = ActualWindowMaterial == WindowMaterial.Integrated;
    }

    protected D2D1Brush GetBrush(Brush brush)
    {
        if (brush >= Brush._Last)
            ArgumentOutOfRangeException.Throw(nameof(brush));
        return UnsafeHelper.AddTypedOffset(ref UnsafeHelper.GetArrayDataReference(_brushes), (nuint)brush);
    }

    protected override void DisposeCore(bool disposing)
    {
        base.DisposeCore(disposing);
        if (disposing)
        {
            DisposeHelper.SwapDisposeAtomic(ref _titleLayout);
            DisposeHelper.SwapDisposeAtomic(ref _titleDescriptionLayout);
            DisposeHelper.DisposeAllUnsafe(in UnsafeHelper.GetArrayDataReference(_brushes), (nuint)Brush._Last);
        }
        SequenceHelper.Clear(_brushes);
    }
}
