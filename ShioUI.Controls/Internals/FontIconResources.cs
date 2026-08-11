using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Runtime.CompilerServices;

using RiceTea.Core;

using ShioUI.Graphics;
using ShioUI.Graphics.Native.Direct2D.Brushes;
using ShioUI.Utils;

namespace ShioUI.Controls.Internals;

internal sealed class FontIconResources : IDisposable
{
    private static readonly FontIconResources _instance = new FontIconResources();

    private readonly FontIcon? _scrollUpIcon, _scrollDownIcon;
    private readonly ConcurrentDictionary<float, FontIcon?> _dropDownIconDict, _checkMarkIconDict;

    private bool _disposed;

    public static FontIconResources Instance => _instance;

    private FontIconResources()
    {
        FontIconFactory factory = FontIconFactory.Instance;
        _scrollUpIcon = GetScrollUpIcon(factory);
        _scrollDownIcon = GetScrollDownIcon(factory);
        _dropDownIconDict = new ConcurrentDictionary<float, FontIcon?>();
        _checkMarkIconDict = new ConcurrentDictionary<float, FontIcon?>();
    }

    private static FontIcon? GetScrollUpIcon(FontIconFactory factory)
    {
        if (factory.TryCreateFluentUIFontIcon(0xEDDB, UIConstantsPrivate.ScrollBarScrollButtonSize, out FontIcon? result) ||
            factory.TryCreateSegoeSymbolFontIcon(0x1F53A, UIConstantsPrivate.ScrollBarScrollButtonSize, out result) ||
            factory.TryCreateWebdingsFontIcon(0xF035, UIConstantsPrivate.ScrollBarScrollButtonSize, out result))
            return result;
        return null;
    }

    private static FontIcon? GetScrollDownIcon(FontIconFactory factory)
    {
        if (factory.TryCreateFluentUIFontIcon(0xEDDC, UIConstantsPrivate.ScrollBarScrollButtonSize, out FontIcon? result) ||
            factory.TryCreateSegoeSymbolFontIcon(0x1F53B, UIConstantsPrivate.ScrollBarScrollButtonSize, out result) ||
            factory.TryCreateWebdingsFontIcon(0xF036, UIConstantsPrivate.ScrollBarScrollButtonSize, out result))
            return result;
        return null;
    }

    private static FontIcon? CreateDropDownIcon(float layoutHeight)
    {
        FontIconFactory factory = FontIconFactory.Instance;
        const uint ComboBoxDropdownCharater = 0xE011;
        SizeF size = new SizeF(layoutHeight, layoutHeight);
        if (factory.TryCreateFluentUIFontIcon(ComboBoxDropdownCharater, size, out FontIcon? result) ||
            factory.TryCreateSegoeSymbolFontIcon(ComboBoxDropdownCharater, size, out result))
            return result;
        return null;
    }

    private static FontIcon? CreateCheckMarkIcon(float layoutHeight)
    {
        FontIconFactory factory = FontIconFactory.Instance;
        SizeF size = new SizeF(layoutHeight, layoutHeight);
        if (factory.TryCreateFluentUIFontIcon(0xE73E, size, out FontIcon? result) ||
            factory.TryCreateSegoeSymbolFontIcon(0xE001, size, out result) ||
            factory.TryCreateWebdingsFontIcon(0x0061, size, out result))
            return result;
        return null;
    }

    private static unsafe FontIcon? GetOrCreateIcon(ConcurrentDictionary<float, FontIcon?> dict, float layoutHeight,
        delegate* managed<float, FontIcon?> createFunc)
    {
        if (layoutHeight < float.Epsilon)
            return null;
        return dict.AddOrUpdate(layoutHeight,
            static (size, func) => ((delegate* managed<float, FontIcon?>)func)(size),
            static (size, old, func) => old ?? ((delegate* managed<float, FontIcon?>)func)(size),
            (nuint)createFunc);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe void DrawDropDownButton(in RegionalRenderingContext context, in RectangleF rect, D2D1Brush brush)
        => GetOrCreateIcon(_dropDownIconDict,
            rect.Height - UIConstants.ElementMargin, &CreateDropDownIcon)?.Render(context, rect, brush);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawScrollBarUpButton(in RegionalRenderingContext context, in RectangleF rect, D2D1Brush brush)
        => _scrollUpIcon?.Render(context, rect, brush);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawScrollBarDownButton(in RegionalRenderingContext context, in RectangleF rect, D2D1Brush brush)
        => _scrollDownIcon?.Render(context, rect, brush);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe void DrawCheckMark(in RegionalRenderingContext context, in RectangleF rect, D2D1Brush brush)
        => GetOrCreateIcon(_checkMarkIconDict,
            rect.Height, &CreateCheckMarkIcon)?.Render(context, rect.Location, brush);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void DisposeCore()
    {
        if (Cells.Exchange(ref _disposed, true))
            return;
        _scrollUpIcon?.Dispose();
        _scrollDownIcon?.Dispose();
        _dropDownIconDict.Clear();
        _checkMarkIconDict.Clear();
    }

    public void Dispose()
    {
        DisposeCore();
        GC.SuppressFinalize(this);
    }
}
