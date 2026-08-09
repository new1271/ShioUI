using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;

using InlineMethod;

using RiceTea.Core;
using RiceTea.Core.Buffers;
using RiceTea.Core.Collections;
using RiceTea.Core.Extensions;
using RiceTea.Core.Helpers;
using RiceTea.Core.Native;
using RiceTea.Core.Structures;

using ShioUI.Graphics;
using ShioUI.Graphics.Helpers;
using ShioUI.Graphics.Native.Direct2D;
using ShioUI.Graphics.Native.Direct2D.Brushes;
using ShioUI.Graphics.Native.DirectWrite;
using ShioUI.Layout;
using ShioUI.Theme;
using ShioUI.Utils;

namespace ShioUI.Controls;

public sealed partial class ListBox : ScrollableElementBase
{
    private static readonly string[] _brushNames = new string[(int)Brush._Last]
    {
        "back",
        "back.disabled",
        "border",
        "fore"
    };
    private static readonly string[] _checkBoxBrushNames = new string[(int)CheckBoxBrush._Last]
    {
        "border",
        "border.hovered" ,
        "border.pressed",
        "border.checked" ,
        "border.hovered.checked" ,
        "border.pressed.checked",
        "mark"
    };

    private readonly D2D1Brush[] _brushes = new D2D1Brush[(int)Brush._Last];
    private readonly D2D1Brush?[] _checkBoxBrushes = new D2D1Brush[(int)CheckBoxBrush._Last];
    private readonly LayoutNode?[] _autoLayoutDefinitions = new LayoutNode?[2];
    private readonly SyncList<bool, BitList> _stateVectorList;
    private readonly SyncList<string, ObservableList<string>> _items;

    private DWriteTextFormat? _format;
    private string _checkBoxThemePrefix;
    private string? _fontName;
    private ListBoxMode _chooseMode;
    private ButtonTriState _buttonState;
    private long _recalcFormat;
    private float _fontSize;
    private int _selectedIndex, _itemHeight;

    public ListBox(IElementContainer parent) : base(parent, "app.listBox")
    {
        _stateVectorList = new SyncList<bool, BitList>(new BitList());

        ObservableList<string> items = new ObservableList<string>();
        items.Updated += Items_Updated;
        items.BeforeAdd += Item_BeforeAdd;
        _items = new SyncList<string, ObservableList<string>>(items);

        ScrollBarType = ScrollBarType.AutoVertial;
        _fontSize = UIConstants.BoxFontSize;
        _selectedIndex = -1;
        _recalcFormat = Booleans.TrueLong;
        _checkBoxThemePrefix = "app.checkBox";
    }

    public void CopySelectedItemsToBuffer(string[] destination, int startIndex, out int itemCopied)
    {
        int length;
        if (startIndex < 0 || startIndex >= (length = destination.Length))
        {
            ArgumentOutOfRangeException.Throw(nameof(startIndex));
            goto Empty;
        }

        SyncList<string, ObservableList<string>> items = _items;
        using (Lock.Scope scope = items.EnterLockScope())
        {
            IList<string> unwrappedItems = items.Items.GetUnderlyingList();
            int count = MathHelper.Min(unwrappedItems.Count, length - startIndex);
            if (count <= 0)
                goto Empty;

            CopySelectedItemsToBufferCore(unwrappedItems, ref destination.AsUnsafeRef()[startIndex], count, out itemCopied);
            return;
        }

    Empty:
        itemCopied = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CopySelectedItemsToBufferCore(IList<string> items, ref string destinationRef, int count, out int itemCopied)
    {
        SyncList<bool, BitList> stateVectorList = _stateVectorList;
        using Lock.Scope scope = stateVectorList.EnterLockScope();

        BitList unwrappedStateVectorList = stateVectorList.Items;
        itemCopied = 0;
        for (int i = 0; i < count; i++)
        {
            if (!unwrappedStateVectorList[i])
                continue;
            UnsafeHelper.AddTypedOffset(ref destinationRef, itemCopied++) = items[i];
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void CopySelectedIndicesToBufferCore(int* destination, int count, out int itemCopied)
    {
        SyncList<bool, BitList> stateVectorList = _stateVectorList;
        using Lock.Scope scope = stateVectorList.EnterLockScope();

        BitList unwrappedStateVectorList = stateVectorList.Items;
        itemCopied = 0;
        for (int i = 0; i < count; i++)
        {
            if (!stateVectorList[i])
                continue;
            destination[itemCopied++] = i;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe TypedNativeMemoryBlock<bool> CreatePooledStateVectorSnapshot(NativeMemoryPool pool, int count)
    {
        SyncList<bool, BitList> stateVectorList = _stateVectorList;
        using Lock.Scope scope = stateVectorList.EnterLockScope();

        TypedNativeMemoryBlock<bool> result = pool.Rent<bool>(count);
        UnsafeHelper.InitBlockUnaligned(result.NativePointer, 0, (uint)count * sizeof(bool));

        count = MathHelper.Min(count, MathHelper.Max(stateVectorList.Count, 0));
        BitList unwrappedStateVectorList = stateVectorList.Items;
        bool* destination = result.NativePointer;
        for (int i = 0; i < count; i++)
            destination[i] = stateVectorList[i];
        return result;
    }

    protected override void ApplyThemeCore(IThemeResourceProvider provider)
    {
        base.ApplyThemeCore(provider);
        string fontName = provider.FontName;
        _fontName = fontName;
        UIElementHelper.ApplyThemeBrushesUnsafe(provider, _brushes, _brushNames, ThemePrefix, (nuint)Brush._Last);
        UIElementHelper.ApplyThemeBrushesUnsafe(provider, _checkBoxBrushes, _checkBoxBrushNames, _checkBoxThemePrefix, (nuint)CheckBoxBrush._Last);
        DisposeHelper.SwapDisposeAtomic(ref _format);
        Atomics.Write(ref _recalcFormat, Booleans.TrueLong);
        Atomics.Write(ref _itemHeight, MathI.Ceiling(FontHeightHelper.GetFontHeight(fontName, _fontSize)) + 2);
        RecalculateHeight();
    }

    protected override D2D1Brush GetBackBrush() => UnsafeHelper.AddTypedOffset(ref UnsafeHelper.GetArrayDataReference(_brushes), (nuint)Brush.BackBrush);

    protected override D2D1Brush GetBackDisabledBrush() => UnsafeHelper.AddTypedOffset(ref UnsafeHelper.GetArrayDataReference(_brushes), (nuint)Brush.BackDisabledBrush);

    protected override D2D1Brush GetBorderBrush() => UnsafeHelper.AddTypedOffset(ref UnsafeHelper.GetArrayDataReference(_brushes), (nuint)Brush.BorderBrush);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private DWriteTextFormat BuildTextFormat()
    {
        DWriteFactory factory = SharedResources.DWriteFactory;
        DWriteTextFormat format = factory.CreateTextFormat(NullSafetyHelper.ThrowIfNull(_fontName), _fontSize);
        format.ParagraphAlignment = DWriteParagraphAlignment.Center;
        format.WordWrapping = DWriteWordWrapping.NoWrap;

        DWriteInlineObject trimmingSign = factory.CreateEllipsisTrimmingSign(format);
        format.SetTrimming(new DWriteTrimming() { Granularity = DWriteTrimmingGranularity.Character }, trimmingSign);
        trimmingSign.Dispose();

        return format;
    }

    private void Items_Updated(object? sender, EventArgs e) => RecalculateHeight();

    private void Item_BeforeAdd(object? sender, BeforeListAddOrRemoveEventArgs<string> e) => _stateVectorList.Add(false);

    public override void Scrolling(int rollStep) => base.Scrolling(rollStep / 4);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool CheckFormatIsNotAvailable([NotNullWhen(false)] DWriteTextFormat? format)
    {
        if (Atomics.Exchange(ref _recalcFormat, Booleans.FalseLong) != Booleans.FalseLong)
        {
            format?.Dispose();
            return true;
        }
        return format is null || format.IsDisposed;
    }

    protected override unsafe bool RenderContent(in RegionalRenderingContext context, D2D1Brush backBrush)
    {
        if (context.HasDirtyCollector)
        {
            RenderBackground(context, backBrush);
            context.MarkAsDirty();
        }

        ref D2D1Brush brushesRef = ref UnsafeHelper.GetArrayDataReference(_brushes);
        DWriteTextFormat? format = _format;
        if (CheckFormatIsNotAvailable(format))
            format = BuildTextFormat();
        SizeF renderSize = context.Size;
        ListBoxMode mode = Mode;
        float itemHeight = RenderingHelper.RoundInPixel(_itemHeight, Window.GetPixelsPerPoint().Y);
        int currentTop = ViewportPoint.Y;
        int startIndex = (int)(currentTop / itemHeight);
        int endIndex = MathI.Ceiling((currentTop + renderSize.Height) / itemHeight);
        Vector2 pointsPerPixel = context.PixelsPerPoint;
        float borderWidth = context.DefaultBorderWidth;

        float itemLeftEdge = borderWidth + 2;
        float textLeftEdge = mode == ListBoxMode.None ? itemLeftEdge : itemLeftEdge * 2 + itemHeight;
        float itemTopEdge = startIndex * itemHeight - currentTop + borderWidth + 2;
        float itemRightEdge = renderSize.Width - borderWidth;
        itemLeftEdge = RenderingHelper.RoundInPixel(itemLeftEdge, pointsPerPixel.X);
        textLeftEdge = RenderingHelper.RoundInPixel(textLeftEdge, pointsPerPixel.X) - itemLeftEdge;
        itemTopEdge = RenderingHelper.RoundInPixel(itemTopEdge, pointsPerPixel.Y);
        float itemWidth = itemRightEdge - itemLeftEdge;
        // itemRightEdge 無須做 round 操作，因為 renderSize.Width 與 borderWidth 均已對齊 pixel

        D2D1Brush textBrush = UnsafeHelper.AddTypedOffset(ref brushesRef, (nuint)Brush.TextBrush);

        using ArrayPool<string>.RentScope itemsScope = ArrayPool<string>.Shared.EnterRentScopeAndCapture(_items);
        int count = itemsScope.Count;
        if (count > 0)
        {
            NativeMemoryPool memoryPool = NativeMemoryPool.Shared;
            TypedNativeMemoryBlock<bool> stateVectorSnapshot = CreatePooledStateVectorSnapshot(memoryPool, count);
            try
            {
                ref readonly string itemsRef = ref itemsScope.GetReferenceOfFirstElement();
                bool* stateVectorSnapshotPointer = stateVectorSnapshot.NativePointer;
                for (int i = startIndex, selectedIndex = _selectedIndex; i <= endIndex && i < count; i++)
                {
                    string item = UnsafeHelper.AddTypedOffsetAsReadOnly(in itemsRef, i);
                    RectF itemBounds = new RectF(itemLeftEdge, itemTopEdge, itemRightEdge, itemTopEdge + itemHeight);
                    using RegionalRenderingContext itemContext = context.WithAxisAlignedClip(itemBounds, D2D1AntialiasMode.Aliased);
                    switch (mode)
                    {
                        case ListBoxMode.None:
                            if (StringHelper.IsNullOrWhiteSpace(item))
                                break;
                            itemContext.DrawText(item, format, RectF.FromXYWH(textLeftEdge, 0, itemWidth, itemHeight), textBrush, D2D1DrawTextOptions.Clip, DWriteMeasuringMode.Natural);
                            break;
                        case ListBoxMode.Any:
                            DrawRadioBox(in itemContext, itemHeight, stateVectorSnapshotPointer[i], selectedIndex == i);
                            goto case ListBoxMode.None;
                        case ListBoxMode.Some:
                            DrawCheckBox(in itemContext, itemHeight, stateVectorSnapshotPointer[i], selectedIndex == i);
                            goto case ListBoxMode.None;
                    }
                    itemTopEdge += itemHeight;
                }
            }
            finally
            {
                memoryPool.Return(stateVectorSnapshot);
            }
        }
        return true;
    }

    private void DrawCheckBox(in RegionalRenderingContext context, float itemHeight, bool checkState, bool isCurrentlyItem)
    {
        RectangleF renderingBounds = CheckBox.GetCheckBoxRenderingBounds(in context, itemHeight);
        CheckBox.DrawCheckBox(context, _checkBoxBrushes, renderingBounds, checkState,
            (ButtonTriState)((uint)_buttonState & UnsafeHelper.Negate(MathHelper.BooleanToUInt32(isCurrentlyItem))));
    }

    private void DrawRadioBox(in RegionalRenderingContext context, float itemHeight, bool isChecked, bool isCurrentlyItem)
        => DrawRadioBox(context, itemHeight, isChecked,
            (ButtonTriState)((uint)_buttonState & UnsafeHelper.Negate(MathHelper.BooleanToUInt32(isCurrentlyItem))));

    private void DrawRadioBox(in RegionalRenderingContext context, float itemHeight, bool isChecked, ButtonTriState state)
    {
        D2D1Brush? backBrush = UnsafeHelper.AddTypedOffset(ref UnsafeHelper.GetArrayDataReference(_checkBoxBrushes),
            (nuint)CheckBoxBrush.BorderBrush + (nuint)state);
        if (backBrush is null)
            return;
        RectF renderingBounds = new RectF(0.0f, 0.0f, itemHeight, itemHeight);
        D2D1DeviceContext deviceContext = context.DeviceContext;
        using RenderingClipScope scope = context.PushPixelAlignedClip(ref renderingBounds, D2D1AntialiasMode.Aliased);
        (D2D1AntialiasMode lastAntialiasMode, deviceContext.AntialiasMode) = (deviceContext.AntialiasMode, D2D1AntialiasMode.PerPrimitive);
        try
        {
            PointF centerPoint = new PointF(renderingBounds.Width * 0.5f, renderingBounds.Height * 0.5f);
            float borderWidth = context.DefaultBorderWidth;
            float radiusX = centerPoint.X - borderWidth * 2.0f;
            float radiusY = radiusX;
            context.DrawEllipse(GetPixelAlignedEllipse(context, ref centerPoint, ref radiusX, ref radiusY), backBrush, borderWidth);
            if (isChecked)
                context.FillEllipse(new D2D1Ellipse(centerPoint, radiusX - borderWidth * 2.0f, radiusY - borderWidth * 2.0f), backBrush);
        }
        finally
        {
            deviceContext.AntialiasMode = lastAntialiasMode;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static D2D1Ellipse GetPixelAlignedEllipse(in RegionalRenderingContext context, ref PointF centerPoint, ref float radiusX, ref float radiusY)
        {
            Vector2 pixelsPerPoint = context.PixelsPerPoint;
            (float centerX, float centerY) = centerPoint;
            PointF predicatedTopLeft = RenderingHelper.CeilingInPixel(new PointF(centerX - radiusX, centerY - radiusY), pixelsPerPoint);
            PointF predicatedBottomRight = RenderingHelper.FloorInPixel(new PointF(centerX + radiusX, centerY + radiusY), pixelsPerPoint);

            radiusX = (predicatedBottomRight.X - predicatedTopLeft.X) * 0.5f;
            radiusY = (predicatedBottomRight.Y - predicatedTopLeft.Y) * 0.5f;
            centerPoint = new PointF(predicatedTopLeft.X + radiusX, predicatedTopLeft.Y + radiusX);
            return new D2D1Ellipse(
                point: centerPoint,
                radiusX: radiusX,
                radiusY: radiusY);
        }
    }

    private void RecalculateHeight() => SurfaceSize = new Size(0, GetPredictedHeight());

    private int GetPredictedHeight() => _items.Count * _itemHeight + UIConstants.ElementMargin;

    private int GetPredictedWidth()
    {
        string? fontName = _fontName;
        if (fontName is null)
            return UIConstants.ElementMargin;

        using ArrayPool<string>.RentScope scope = ArrayPool<string>.Shared.EnterRentScopeAndCapture(_items);
        int count = scope.Count;
        if (count <= 0)
            return UIConstants.ElementMargin;

        using DWriteTextFormat format = SharedResources.DWriteFactory.CreateTextFormat(fontName, _fontSize);
        int maxVal = 0;
        ref string scopeRef = ref scope.GetReferenceOfFirstElement();
        for (int i = 0; i < count; i++)
            maxVal = MathHelper.Max(maxVal, GraphicsUtils.MeasureTextWidthAsInt(UnsafeHelper.AddTypedOffset(ref scopeRef, i), format));
        return maxVal + UIConstants.ElementMargin;
    }

    protected override void OnMouseMove(in MouseEventArgs args)
    {
        base.OnMouseMove(args);
        if (!args.IsInSpecificSize(ContentSize))
        {
            if (_buttonState != ButtonTriState.None)
            {
                _buttonState = ButtonTriState.None;
                Update();
            }
            return;
        }
        if (Mode == ListBoxMode.None)
            return;
        int selectedIndex = (int)((args.Y + ViewportPoint.Y) / _itemHeight);
        if (selectedIndex >= Items.Count) selectedIndex = -1;
        ButtonTriState state = ButtonTriState.None;
        if (selectedIndex > -1)
        {
            if (_buttonState != ButtonTriState.Pressed)
                state = ButtonTriState.Hovered;
        }
        else
        {
            state = ButtonTriState.None;
        }
        bool changed = false;
        if (_selectedIndex != selectedIndex)
        {
            _selectedIndex = selectedIndex;
            changed = true;
        }
        if (_buttonState != state)
        {
            _buttonState = state;
            changed = true;
        }
        if (changed)
            Update();
    }

    protected override void OnMouseDown(ref HandleableMouseEventArgs args)
    {
        base.OnMouseDown(ref args);
        if (args.Handled || Mode == ListBoxMode.None || !args.Buttons.HasFlagFast(MouseButtons.LeftButton))
            return;
        args.Handle();
        if (_buttonState == ButtonTriState.Hovered)
            _buttonState = ButtonTriState.Pressed;
        Update();
    }

    protected override void OnMouseUp(in MouseEventArgs args)
    {
        base.OnMouseUp(args);
        ListBoxMode mode = Mode;
        if (mode == ListBoxMode.None || _buttonState != ButtonTriState.Pressed)
            return;
        if (args.IsInSpecificSize(ContentSize))
            _buttonState = ButtonTriState.Hovered;
        else
            _buttonState = ButtonTriState.None;
        switch (Mode)
        {
            case ListBoxMode.Any:
                ClearCheckStateCore();
                goto case ListBoxMode.Some;
            case ListBoxMode.Some:
                int selectedIndex = _selectedIndex;
                RevertCheckStateCore(selectedIndex);
                OnSelectedIndicesChanged();
                break;
        }
        Update();
    }

    public bool IsChecked(int index) => Mode != ListBoxMode.None && GetCheckStateCore(index);

    public void SetChecked(int index, bool value)
    {
        ListBoxMode mode = Mode;
        switch (mode)
        {
            case ListBoxMode.Any:
                if (IsChecked(index) == value)
                    return;
                ClearCheckStateCore();
                goto case ListBoxMode.Some;
            case ListBoxMode.Some:
                SetCheckStateCore(index, value);
                break;
        }
    }

    private void OnSelectedIndicesChanged() => SelectedIndicesChanged?.Invoke(this, EventArgs.Empty);

    protected override void DisposeCore(bool disposing)
    {
        base.DisposeCore(disposing);
        if (disposing)
        {
            DisposeHelper.SwapDisposeAtomic(ref _format);
            DisposeHelper.DisposeAllUnsafe(in UnsafeHelper.GetArrayDataReference(_brushes), (nuint)Brush._Last);
        }
        SequenceHelper.Clear(_brushes);
    }

    protected override void OnScrollBarUpButtonClicked() => Scrolling(-MathI.Ceiling(_itemHeight));

    protected override void OnScrollBarDownButtonClicked() => Scrolling(MathI.Ceiling(_itemHeight));

    [Inline(InlineBehavior.Remove)]
    private bool GetCheckStateCore(int index) => _stateVectorList[index];

    [Inline(InlineBehavior.Remove)]
    private void SetCheckStateCore(int index, bool state) => _stateVectorList[index] = state;

    [Inline(InlineBehavior.Remove)]
    private void RevertCheckStateCore(int index)
        => RevertCheckStateCore(_stateVectorList, index);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void RevertCheckStateCore(SyncList<bool, BitList> stateVectorList, int index)
    {
        using Lock.Scope scope = stateVectorList.EnterLockScope();
        BitList list = stateVectorList.Items;
        list[index] = !list[index];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ClearCheckStateCore() => ClearCheckStateCore(_stateVectorList);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ClearCheckStateCore(SyncList<bool, BitList> stateVectorList)
    {
        using Lock.Scope scope = stateVectorList.EnterLockScope();
        BitList list = stateVectorList.Items;

        list.SetAllBitsAsFalse();
    }
}
