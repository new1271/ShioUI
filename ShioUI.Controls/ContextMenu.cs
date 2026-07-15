using System;
using System.Drawing;
using System.Numerics;

using RiceTea.Core;
using RiceTea.Core.Extensions;
using RiceTea.Core.Helpers;
using RiceTea.Core.Structures;

using ShioUI.Graphics;
using ShioUI.Graphics.Helpers;
using ShioUI.Graphics.Native.Direct2D;
using ShioUI.Graphics.Native.Direct2D.Brushes;
using ShioUI.Graphics.Native.DirectWrite;
using ShioUI.Theme;
using ShioUI.Utils;

namespace ShioUI.Controls;

public sealed partial class ContextMenu : PopupElementBase, ICheckableDisposable, IGlobalMouseMoveHandler, IKeyboardInteractHandler
{
    private static readonly string[] _brushNames = new string[(int)Brush._Last]
    {
        "back",
        "back.hovered",
        "back.pressed",
        "border",
        "fore",
        "fore.inactive",
        "fore.hovered"
    };

    private readonly D2D1Brush[] _brushes = new D2D1Brush[(int)Brush._Last];
    private readonly DWriteTextLayout?[] _layouts;
    private readonly Point _initialLocation;

    private SizeF _itemSize;
    private int _hoveredIndex;
    private bool _isPressed;

    public ContextMenu(IElementContainer parent, Item[] items, Point initialLocation) : base(parent, "app.contextMenu")
    {
        MenuItems = items;
        _initialLocation = initialLocation;
        _layouts = new DWriteTextLayout[items.Length];
        WeakReference<ContextMenu> reference = new WeakReference<ContextMenu>(this);
        LeftExpression = new DefaultLeftNode(reference);
        TopExpression = new DefaultTopNode(reference);
        WidthExpression = new DefaultWidthNode(reference);
        HeightExpression = new DefaultHeightNode(reference);
    }

    protected override void ApplyThemeCore(IThemeResourceProvider provider)
    {
        UIElementHelper.ApplyThemeBrushesUnsafe(provider, _brushes, _brushNames, ThemePrefix, (nuint)Brush._Last);
        DWriteFactory factory = SharedResources.DWriteFactory;

        Item[] items = MenuItems;
        float itemHeight = 0f, itemWidth = 0f;
        Vector2 pixelsPerPoint = Window.GetPixelsPerPoint();
        int count = items.Length;
        DWriteTextLayout?[] layouts = _layouts;
        using DWriteTextFormat format = factory.CreateTextFormat(provider.FontName, UIConstants.BoxFontSize);
        format.ParagraphAlignment = DWriteParagraphAlignment.Center;

        ref Item itemArrayRef = ref UnsafeHelper.GetArrayDataReference(items);
        ref DWriteTextLayout? layoutArrayRef = ref UnsafeHelper.GetArrayDataReference(layouts);
        for (int i = 0; i < count; i++)
        {
            string text = UnsafeHelper.AddTypedOffset(ref itemArrayRef, i).Text;
            DWriteTextLayout layout = factory.CreateTextLayout(items[i].Text, format);
            DWriteTextMetrics metrics = layout.GetMetrics();
            itemWidth = MathHelper.Max(itemWidth, metrics.Width);
            itemHeight = MathHelper.Max(itemHeight, metrics.Height);
            DisposeHelper.SwapDispose(ref UnsafeHelper.AddTypedOffset(ref layoutArrayRef, i), layout);
        }
        itemWidth = RenderingHelper.CeilingInPixel(itemWidth, pixelsPerPoint.X);
        itemHeight = RenderingHelper.CeilingInPixel(itemHeight + UIConstants.ElementMarginHalf, pixelsPerPoint.Y);
        for (int i = 0; i < count; i++)
        {
            DWriteTextLayout? layout = UnsafeHelper.AddTypedOffset(ref layoutArrayRef, i);
            DebugHelper.ThrowIf(layout is null);
            layout.MaxWidth = itemWidth;
            layout.MaxHeight = itemHeight;
        }
        _itemSize = new SizeF(itemWidth, itemHeight);
    }

    protected override bool IsBackgroundOpaqueCore() => GraphicsUtils.CheckBrushIsSolid(
        UnsafeHelper.AddTypedOffset(ref UnsafeHelper.GetArrayDataReference(_brushes), (nuint)Brush.BackBrush));

    protected override bool RenderCore(in RegionalRenderingContext context)
    {
        SizeF renderSize = context.Size;
        float borderWidth = context.DefaultBorderWidth;
        ref D2D1Brush brushesRef = ref UnsafeHelper.GetArrayDataReference(_brushes);
        D2D1Brush backBrush = UnsafeHelper.AddTypedOffset(ref brushesRef, (nuint)Brush.BackBrush),
            borderBrush = UnsafeHelper.AddTypedOffset(ref brushesRef, (nuint)Brush.BorderBrush);
        RenderBackground(context, backBrush);
        DWriteTextLayout?[] layouts = _layouts;
        int length;
        if ((length = layouts.Length) > 0)
        {
            ref Item itemArrayRef = ref UnsafeHelper.GetArrayDataReference(MenuItems);
            ref DWriteTextLayout? layoutArrayRef = ref UnsafeHelper.GetArrayDataReference(layouts);
            D2D1Brush foreBrush = UnsafeHelper.AddTypedOffset(ref brushesRef, (nuint)Brush.TextBrush),
                foreDisabledBrush = UnsafeHelper.AddTypedOffset(ref brushesRef, (nuint)Brush.TextInactiveBrush);
            int hoveredIndex = _hoveredIndex;
            float itemLeft = borderWidth,
                textLeft = itemLeft + UIConstants.ElementMarginHalf,
                itemTop = borderWidth,
                itemRight = renderSize.Width - borderWidth;
            int i = 0;
            do
            {
                ref DWriteTextLayout? layoutRef = ref UnsafeHelper.AddTypedOffset(ref layoutArrayRef, i);
                DWriteTextLayout? layout = InterlockedHelper.Exchange(ref layoutRef, null);
                if (layout is null)
                    continue;
                try
                {
                    bool isEnabled = UnsafeHelper.AddTypedOffset(ref itemArrayRef, i).Enabled;
                    float itemHeight = layout.MaxHeight;
                    D2D1Brush currentForeBrush;
                    if (isEnabled)
                    {
                        if (i == hoveredIndex && isEnabled)
                        {
                            using RenderingClipScope scope = context.PushAxisAlignedClip(
                                new RectF(itemLeft, itemTop, itemRight, itemTop + itemHeight), D2D1AntialiasMode.Aliased);
                            D2D1Brush currentBackBrush = UnsafeHelper.AddTypedOffset(ref brushesRef, (nuint)Brush.BackHoveredBrush + MathHelper.BooleanToNativeUnsigned(_isPressed));
                            currentForeBrush = UnsafeHelper.AddTypedOffset(ref brushesRef, (nuint)Brush.TextHoveredBrush);
                            RenderBackground(context, currentBackBrush);
                        }
                        else
                            currentForeBrush = foreBrush;
                    }
                    else
                        currentForeBrush = foreDisabledBrush;
                    context.DrawTextLayout(new PointF(textLeft, itemTop), layout, currentForeBrush, D2D1DrawTextOptions.NoSnap);
                    itemTop += itemHeight;
                }
                finally
                {
                    DisposeHelper.NullSwapOrDispose(ref layoutRef, layout);
                }
            } while (++i < length);

        }
        context.DrawBorder(borderBrush);
        return true;
    }

    protected override void OnMouseDownGlobally(in MouseEventArgs args)
    {
        base.OnMouseDownGlobally(args);
        if (!args.Buttons.HasFlagFast(MouseButtons.LeftButton))
            return;

        if (!Bounds.Contains(args.Location))
        {
            Close();
            return;
        }

        _isPressed = true;
        Update();
    }

    protected override void OnMouseUpGlobally(in MouseEventArgs args)
    {
        base.OnMouseUpGlobally(args);
        if (!args.Buttons.HasFlagFast(MouseButtons.LeftButton) || !Bounds.Contains(args.Location))
            return;
        _isPressed = false;
        int hoveredIndex = _hoveredIndex;
        Item[] items = MenuItems;
        if (items.Length > hoveredIndex && hoveredIndex >= 0)
        {
            Item item = items[hoveredIndex];
            ItemClicked?.Invoke(this, EventArgs.Empty);
            item.OnClick();
            Close();
        }
    }

    void IGlobalMouseMoveHandler.OnMouseMoveGlobally(in MouseEventArgs args)
    {
        Rectangle bounds = Bounds;
        int hoveredIndex;

        if (!bounds.Contains(args.Location))
            hoveredIndex = -1;
        else
        {
            float itemHeight = _itemSize.Height;
            hoveredIndex = (int)((args.Y - Location.Y) / itemHeight);
            if (MenuItems.Length <= hoveredIndex)
                hoveredIndex = -1;
        }

        if (_hoveredIndex != hoveredIndex)
        {
            _hoveredIndex = hoveredIndex;
            Update();
        }
    }

    void IKeyboardInteractHandler.OnKeyDown(ref KeyEventArgs args)
    {
        if (args.Key != VirtualKey.Escape ||
            Keys.IsAltPressed() || Keys.IsControlPressed() || Keys.IsShiftPressed())
            return;
        args.Handle();
        Close();
    }

    void IKeyboardInteractHandler.OnKeyUp(ref KeyEventArgs args)
    {
        //Do nothing
    }

    protected override void DisposeCore(bool disposing)
    {
        base.DisposeCore(disposing);
        DWriteTextLayout?[] layouts = _layouts;
        if (disposing)
        {
            DisposeHelper.DisposeAll(layouts);
            DisposeHelper.DisposeAllUnsafe(in UnsafeHelper.GetArrayDataReference(_brushes), (nuint)Brush._Last);
        }
        SequenceHelper.Clear(layouts);
        SequenceHelper.Clear(_brushes);
    }
}
