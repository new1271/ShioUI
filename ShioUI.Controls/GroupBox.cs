using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Threading;

using InlineMethod;

using RiceTea.Core;
using RiceTea.Core.Extensions;
using RiceTea.Core.Helpers;
using RiceTea.Core.Structures;

using ShioUI.Controls.Internals;
using ShioUI.Graphics;
using ShioUI.Graphics.Native.Direct2D;
using ShioUI.Graphics.Native.Direct2D.Brushes;
using ShioUI.Graphics.Native.DirectWrite;
using ShioUI.Layout;
using ShioUI.Theme;
using ShioUI.Utils;

namespace ShioUI.Controls;

public sealed partial class GroupBox : UIElement, IAppendableElementContainer
{
    private const int ContentPageLeftPadding = UIConstants.ElementMargin;
    private const int ContentPageRightPadding = UIConstants.ElementMargin;
    private const int ContentPageBottomPadding = UIConstants.ElementMargin;
    private const int BorderedTitleExtraWidth = UIConstants.ElementMarginDouble;
    private const int CardTitleDescriptionExtraLeftPadding = UIConstants.ElementMarginHalf;
    private readonly record struct Layouts(DWriteTextLayout? Title, DWriteTextLayout? TitleDescription);

    private static readonly string[] _brushNames = new string[(int)Brush._Last]
    {
        "border",
        "fore",
        "fore.description",
        "card.back",
        "card.fore",
        "card.fore.description",
    };

    private readonly D2D1Brush[] _brushes = new D2D1Brush[(int)Brush._Last];
    private readonly LayoutNode?[] _autoLayoutDefinitions = new LayoutNode?[2];
    private readonly UIElementCollection _children;

    private WeakReference<GroupBox>? _reference;
    private DWriteTextLayout? _titleLayout, _titleDescriptionLayout;
    private string? _fontName;
    private string _title, _titleDescription;
    private ContentPageScopeParams _contentPageScopeParams;
    private long _redrawTypeRaw, _rawUpdateFlags;
    private uint _mode;
    private int _titleHeight;

    public GroupBox(IElementContainer parent) : base(parent, "app.groupBox")
    {
        _children = new UIElementCollection(this);
        _title = string.Empty;
        _titleDescription = string.Empty;
        _redrawTypeRaw = (long)RedrawType.RedrawAllContent;
        _rawUpdateFlags = (long)RenderObjectUpdateFlags.FlagsAllTrue;

        EnablePartialRendering = true;
    }

    public ContentPageScope EnterContentPageScope()
    {
        ref ContentPageScopeParams @params = ref _contentPageScopeParams;
        if (@params.PageLeftDefinition is null)
        {
            WeakReference<GroupBox> reference = GetWeakReference();
            @params = new()
            {
                PageLeftDefinition = LayoutNode.Fixed(ContentPageLeftPadding),
                PageTopDefinition = new ContentTopNode(reference),
                PageRightDefinition = new ContentRightNode(reference),
                PageBottomDefinition = new ContentBottomNode(reference),
                PageWidthDefinition = new ContentWidthNode(reference),
                PageHeightDefinition = new ContentHeightNode(reference)
            };
        }
        return ContentPageScope.Create(this, @params);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private WeakReference<GroupBox> GetWeakReference()
    {
        WeakReference<GroupBox>? reference = Atomics.Read(ref _reference);
        if (reference is null)
        {
            reference = new WeakReference<GroupBox>(this);
            WeakReference<GroupBox>? oldReference = Atomics.CompareExchange(ref _reference, reference, null);
            if (oldReference is not null)
                reference = oldReference;
        }
        return reference;
    }

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
        UIElementHelper.ApplyThemeBrushesUnsafe(provider, _brushes, _brushNames, ThemePrefix, (nuint)Brush._Last);
        UIElementHelper.ApplyThemeToElements(provider, _children);
        string fontName = provider.FontName;
        _fontName = fontName;
        _titleHeight = MathI.Ceiling(FontHeightHelper.GetFontHeight(fontName, UIConstants.DefaultFontSize));
        Update(RenderObjectUpdateFlags.Format, RedrawType.RedrawAllContent);
    }

    protected override bool IsBackgroundOpaqueCore()
    {
        if (Mode == GroupBoxMode.Card)
            return GraphicsUtils.CheckBrushIsSolid(_brushes.AsUnsafeRef()[(nuint)Brush.CardBackBrush]);
        else
            return false;
    }

    void IElementContainer.RenderBackground(UIElement element, in RegionalRenderingContext context)
    {
        if (Mode == GroupBoxMode.Card)
            RenderBackground(context, _brushes.AsUnsafeRef()[(nuint)Brush.CardBackBrush]);
        else
            RenderBackground(context);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void Update() => Update(RedrawType.RedrawAllContent);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Update(RedrawType type)
    {
        if (type == RedrawType.NoRedraw)
            return;
        Atomics.Or(ref _redrawTypeRaw, (long)type);
        UpdateCore();
    }

    [Inline(InlineBehavior.Remove)]
    private void Update(RenderObjectUpdateFlags flags, RedrawType redrawType)
    {
        Atomics.Or(ref _rawUpdateFlags, (long)flags);
        Update(redrawType);
    }

    [Inline(InlineBehavior.Remove)]
    private RedrawType GetRedrawTypeAndReset()
        => (RedrawType)Interlocked.Exchange(ref _redrawTypeRaw, (long)RedrawType.NoRedraw);

    [Inline(InlineBehavior.Remove)]
    private RenderObjectUpdateFlags GetAndCleanRenderObjectUpdateFlags()
        => (RenderObjectUpdateFlags)Interlocked.Exchange(ref _rawUpdateFlags, default);

    public override bool NeedRefresh()
    {
        if (_redrawTypeRaw > (long)RedrawType.NoRedraw)
            return true;
        return Atomics.Read(ref _redrawTypeRaw) > (long)RedrawType.NoRedraw;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Layouts GetLayouts(RenderObjectUpdateFlags flags, GroupBoxMode mode)
    {
        DWriteTextLayout? titleLayout = _titleLayout;
        DWriteTextLayout? titleDescriptionLayout = _titleDescriptionLayout;

        string fontName = NullSafetyHelper.ThrowIfNull(_fontName);
        int titleHeight = _titleHeight;

        DWriteFactory factory = SharedResources.DWriteFactory;
        if (flags.HasFlagFast(RenderObjectUpdateFlags.Title))
        {
            string title = _title;
            if (title.Length <= 0)
            {
                if (titleLayout is not null)
                {
                    titleLayout.Dispose();
                    titleLayout = null;
                }
            }
            else
            {
                DWriteTextFormat? format = titleLayout;
                if (CheckFormatIsNotAvailable(format, flags))
                {
                    format = mode switch
                    {
                        GroupBoxMode.Card => TextFormatHelper.CreateTextFormat(TextAlignment.BottomCenter, fontName, UIConstants.CardTitleFontSize,
                                                    DWriteFontWeight.Bold, DWriteFontStyle.Normal),
                        _ => TextFormatHelper.CreateTextFormat(TextAlignment.BottomCenter, fontName, UIConstants.DefaultFontSize,
                                                    DWriteFontWeight.Normal, DWriteFontStyle.Normal),
                    };
                }
                titleLayout = factory.CreateTextLayout(title, format);
                format.Dispose();
                titleLayout.MaxWidth = titleLayout.GetMetrics().Width + mode switch
                {
                    GroupBoxMode.Card => 0,
                    _ => BorderedTitleExtraWidth,
                };
                titleLayout.MaxHeight = titleHeight;
                _titleLayout = titleLayout;
            }
        }
        if (flags.HasFlagFast(RenderObjectUpdateFlags.TitleDescription))
        {
            string titleDescription = _titleDescription;
            if (titleDescription.Length <= 0)
            {
                if (titleDescriptionLayout is not null)
                {
                    titleDescriptionLayout.Dispose();
                    titleDescriptionLayout = null;
                }
            }
            else
            {
                DWriteTextFormat? format = titleDescriptionLayout;
                if (CheckFormatIsNotAvailable(format, flags))
                {
                    format = mode switch
                    {
                        GroupBoxMode.Card => TextFormatHelper.CreateTextFormat(TextAlignment.BottomCenter, fontName, UIConstants.CardTitleDescriptionFontSize,
                                                    DWriteFontWeight.Bold, DWriteFontStyle.Normal),
                        _ => TextFormatHelper.CreateTextFormat(TextAlignment.BottomCenter, fontName, UIConstants.DescriptionFontSize,
                                                    DWriteFontWeight.Normal, DWriteFontStyle.Normal),
                    };
                }
                titleDescriptionLayout = factory.CreateTextLayout(titleDescription, format);
                format.Dispose();
                titleDescriptionLayout.MaxWidth = titleDescriptionLayout.GetMetrics().Width + mode switch
                {
                    GroupBoxMode.Card => 0,
                    _ => BorderedTitleExtraWidth,
                };
                titleDescriptionLayout.MaxHeight = titleHeight;
                _titleDescriptionLayout = titleDescriptionLayout;
            }
        }
        return new(titleLayout, titleDescriptionLayout);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool CheckFormatIsNotAvailable([NotNullWhen(false)] DWriteTextFormat? format, RenderObjectUpdateFlags flags)
    {
        if (format is null || format.IsDisposed)
            return true;
        if (flags.HasFlagFast(RenderObjectUpdateFlags.Format))
        {
            format.Dispose();
            return true;
        }
        return false;
    }

    bool IElementContainer.IsBackgroundOpaque(UIElement element) => IsBackgroundOpaque();

    protected override bool RenderCore(in RegionalRenderingContext context)
    {
        RedrawType redrawType = GetRedrawTypeAndReset();
        if (!context.HasDirtyCollector) // Force redraw
            redrawType = RedrawType.RedrawAllContent;
        else if (redrawType == RedrawType.NoRedraw)
            return true;
        GroupBoxMode mode = (GroupBoxMode)Atomics.Read(ref _mode);
        Layouts layouts = GetLayouts(GetAndCleanRenderObjectUpdateFlags(), mode);
        switch (redrawType)
        {
            case RedrawType.RedrawAllContent:
                {
                    if (mode == GroupBoxMode.Card)
                    {
                        RenderBackground(context, _brushes.AsUnsafeRef()[(nuint)Brush.CardBackBrush]);
                        RenderTitle_Card(context, layouts, incremental: false);
                    }
                    else
                    {
                        RenderBackground(context);
                        SizeF renderSize = context.Size;
                        RectF borderBounds = new RectF(0, _titleHeight * 0.5f, renderSize.Width, renderSize.Height);
                        context.DrawBorder(borderBounds, _brushes.AsUnsafeRef()[(nuint)Brush.BorderBrush]);
                        RenderTitle_Bordered(context, layouts, incremental: false);
                    }
                    context.MarkAsDirty();
                }
                break;
            case RedrawType.RedrawTitle:
                if (mode == GroupBoxMode.Card)
                    RenderTitle_Card(context, layouts, incremental: true);
                else
                    RenderTitle_Bordered(context, layouts, incremental: true);
                break;
        }
        return true;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void RenderTitle_Bordered(in RegionalRenderingContext context, in Layouts layouts, bool incremental)
    {
        (DWriteTextLayout? layout, DWriteTextLayout? descriptionLayout) = layouts;

        RectF bounds;
        if (layout is null)
        {
            if (descriptionLayout is null)
                return;
            else
            {
                bounds = RectF.FromXYWH(UIConstants.ElementMargin, 0, descriptionLayout.MaxWidth, descriptionLayout.MaxHeight);
                using RenderingClipScope scope = context.PushPixelAlignedClip(ref bounds, D2D1AntialiasMode.Aliased);
                RenderBackground(context);
                context.DrawTextLayout(bounds.Location, descriptionLayout, _brushes.AsUnsafeRef()[(nuint)Brush.TitleDescriptionBrush], D2D1DrawTextOptions.None | D2D1DrawTextOptions.NoSnap);
            }
        }
        else
        {
            if (descriptionLayout is null)
            {
                bounds = RectF.FromXYWH(UIConstants.ElementMargin, 0, layout.MaxWidth, layout.MaxHeight);
                using RenderingClipScope scope = context.PushPixelAlignedClip(ref bounds, D2D1AntialiasMode.Aliased);
                RenderBackground(context);
                context.DrawTextLayout(bounds.Location, layout, _brushes.AsUnsafeRef()[(nuint)Brush.TitleBrush], D2D1DrawTextOptions.None | D2D1DrawTextOptions.NoSnap);
            }
            else
            {
                float titleWidth = layout.MaxWidth, titleDescriptionWidth = descriptionLayout.MaxWidth;
                bounds = RectF.FromXYWH(UIConstants.ElementMargin, 0, titleWidth + titleDescriptionWidth -
                    BorderedTitleExtraWidth / 2, layout.MaxHeight);
                using RenderingClipScope scope = context.PushPixelAlignedClip(ref bounds, D2D1AntialiasMode.Aliased);
                RenderBackground(context);
                ref D2D1Brush brushesRef = ref UnsafeHelper.GetArrayDataReference(_brushes);
                context.DrawTextLayout(bounds.Location, layout, UnsafeHelper.AddTypedOffset(ref brushesRef, (nuint)Brush.TitleBrush),
                    D2D1DrawTextOptions.None | D2D1DrawTextOptions.NoSnap);
                context.DrawTextLayout(new PointF(bounds.X + titleWidth - BorderedTitleExtraWidth / 2, bounds.Y), layout,
                    UnsafeHelper.AddTypedOffset(ref brushesRef, (nuint)Brush.TitleDescriptionBrush), D2D1DrawTextOptions.None | D2D1DrawTextOptions.NoSnap);
            }
        }
        if (incremental)
            context.MarkAsDirty(bounds);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void RenderTitle_Card(in RegionalRenderingContext context, in Layouts layouts, bool incremental)
    {
        (DWriteTextLayout? layout, DWriteTextLayout? descriptionLayout) = layouts;

        RectF bounds;
        if (layout is null)
        {
            if (descriptionLayout is null)
                return;
            else
            {
                bounds = RectF.FromXYWH(UIConstants.ElementMarginDouble, UIConstants.ElementMarginDouble, descriptionLayout.MaxWidth, descriptionLayout.MaxHeight);
                using RenderingClipScope scope = context.PushPixelAlignedClip(ref bounds, D2D1AntialiasMode.Aliased);
                ref D2D1Brush brushesRef = ref UnsafeHelper.GetArrayDataReference(_brushes);
                if (incremental)
                    RenderBackground(context, UnsafeHelper.AddTypedOffset(ref brushesRef, (nuint)Brush.CardBackBrush));
                context.DrawTextLayout(bounds.Location, descriptionLayout, UnsafeHelper.AddTypedOffset(ref brushesRef, (nuint)Brush.CardTitleDescriptionBrush),
                    D2D1DrawTextOptions.None | D2D1DrawTextOptions.NoSnap);
            }
        }
        else
        {
            ref D2D1Brush brushesRef = ref UnsafeHelper.GetArrayDataReference(_brushes);

            if (descriptionLayout is null)
            {
                bounds = RectF.FromXYWH(UIConstants.ElementMarginDouble, UIConstants.ElementMarginDouble, layout.MaxWidth, layout.MaxHeight);
                using RenderingClipScope scope = context.PushPixelAlignedClip(ref bounds, D2D1AntialiasMode.Aliased);
                if (incremental)
                    RenderBackground(context, UnsafeHelper.AddTypedOffset(ref brushesRef, (nuint)Brush.CardBackBrush));
                context.DrawTextLayout(bounds.Location, layout, UnsafeHelper.AddTypedOffset(ref brushesRef, (nuint)Brush.CardTitleBrush),
                    D2D1DrawTextOptions.None | D2D1DrawTextOptions.NoSnap);
            }
            else
            {
                float titleWidth = layout.MaxWidth, titleDescriptionWidth = descriptionLayout.MaxWidth;
                bounds = RectF.FromXYWH(UIConstants.ElementMarginDouble, UIConstants.ElementMarginDouble,
                    titleWidth + titleDescriptionWidth + CardTitleDescriptionExtraLeftPadding, layout.MaxHeight);
                using RenderingClipScope scope = context.PushPixelAlignedClip(ref bounds, D2D1AntialiasMode.Aliased);
                if (incremental)
                    RenderBackground(context, UnsafeHelper.AddTypedOffset(ref brushesRef, (nuint)Brush.CardBackBrush));
                context.DrawTextLayout(bounds.Location, layout, UnsafeHelper.AddTypedOffset(ref brushesRef, (nuint)Brush.CardTitleBrush),
                    D2D1DrawTextOptions.None | D2D1DrawTextOptions.NoSnap);
                context.DrawTextLayout(new PointF(bounds.X + titleWidth + CardTitleDescriptionExtraLeftPadding, bounds.Y), descriptionLayout,
                    UnsafeHelper.AddTypedOffset(ref brushesRef, (nuint)Brush.CardTitleDescriptionBrush), D2D1DrawTextOptions.None | D2D1DrawTextOptions.NoSnap);
            }
        }
        if (incremental)
            context.MarkAsDirty(bounds);
    }

    [Inline(InlineBehavior.Remove)]
    private static int GetContentPageLeftCore() => ContentPageLeftPadding;

    [Inline(InlineBehavior.Remove)]
    private int GetContentPageTopCore()
    {
        int titleHeight = Atomics.Read(ref _titleHeight);
        return (GroupBoxMode)Atomics.Read(ref _mode) switch
        {
            GroupBoxMode.Card => titleHeight + UIConstants.ElementMargin * 3,
            _ => titleHeight
        };
    }

    [Inline(InlineBehavior.Remove)]
    private static int GetContentPageRightCore(int width) => width - ContentPageRightPadding;

    [Inline(InlineBehavior.Remove)]
    private static int GetContentPageBottomCore(int height) => height - ContentPageBottomPadding;

    [Inline(InlineBehavior.Remove)]
    private static int GetContentPageWidthCore(int width) => width - (ContentPageLeftPadding + ContentPageRightPadding);

    [Inline(InlineBehavior.Remove)]
    private int GetContentPageHeightCore(int height) => height - (GetContentPageTopCore() + ContentPageBottomPadding);

    IEnumerable<UIElement?> IElementContainer.GetElements() => _children;

    IEnumerable<UIElement?> IElementContainer.GetActiveElements() => _children;

    protected override void DisposeCore(bool disposing)
    {
        base.DisposeCore(disposing);
        if (disposing)
        {
            DisposeHelper.SwapDisposeAtomic(ref _titleLayout);
            DisposeHelper.DisposeAllUnsafe(in UnsafeHelper.GetArrayDataReference(_brushes), (nuint)Brush._Last);
            _children.Dispose();
        }
        SequenceHelper.Clear(_brushes);
    }
}
