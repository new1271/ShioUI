using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using RiceTea.Core.Buffers;
using RiceTea.Core.Extensions;
using RiceTea.Core.Helpers;

using ShioUI.Graphics.Native.Direct2D.Brushes;
using ShioUI.Theme;

namespace ShioUI.Utils;

partial class UIElementHelper
{
    public static void ApplyThemeBrushes(IThemeResourceProvider provider, D2D1Brush?[] brushes, string[] nodes)
    {
        int length = brushes.Length;
        if (length != nodes.Length)
            throw new ArgumentException("The length of " + nameof(nodes) + " must equals to the length of " + nameof(brushes) + " !");
        if (length <= 0)
            return;
        ApplyThemeBrushesUnsafe(provider, brushes, nodes, (nuint)length);
    }

    public static void ApplyThemeBrushes(IThemeResourceProvider provider, D2D1Brush?[] brushes, string[] nodes, string nodePrefix)
    {
        int length = brushes.Length;
        if (length != nodes.Length)
            throw new ArgumentException("The length of " + nameof(nodes) + " must equals to the length of " + nameof(brushes) + " !");
        if (length <= 0)
            return;
        ApplyThemeBrushesUnsafe(provider, brushes, nodes, nodePrefix, (nuint)length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ApplyThemeBrushesUnsafe(IThemeResourceProvider provider, D2D1Brush?[] brushes, string[] nodes, nuint length)
    {
        ref D2D1Brush? brushesRef = ref UnsafeHelper.GetArrayDataReference(brushes);
        ref readonly string nodesRef = ref UnsafeHelper.GetArrayDataReference(nodes);
        int i;
        for (i = 0; length >= 4; length -= 4, i += 4)
        {
            ApplyThemeBrush(provider, ref UnsafeHelper.AddTypedOffset(ref brushesRef, i), UnsafeHelper.AddTypedOffsetAsReadOnly(in nodesRef, i));
            ApplyThemeBrush(provider, ref UnsafeHelper.AddTypedOffset(ref brushesRef, i + 1), UnsafeHelper.AddTypedOffsetAsReadOnly(in nodesRef, i + 1));
            ApplyThemeBrush(provider, ref UnsafeHelper.AddTypedOffset(ref brushesRef, i + 2), UnsafeHelper.AddTypedOffsetAsReadOnly(in nodesRef, i + 2));
            ApplyThemeBrush(provider, ref UnsafeHelper.AddTypedOffset(ref brushesRef, i + 3), UnsafeHelper.AddTypedOffsetAsReadOnly(in nodesRef, i + 3));
        }
        switch (length)
        {
            case 3:
                ApplyThemeBrush(provider, ref UnsafeHelper.AddTypedOffset(ref brushesRef, i + 2), UnsafeHelper.AddTypedOffsetAsReadOnly(in nodesRef, i + 2));
                goto case 2;
            case 2:
                ApplyThemeBrush(provider, ref UnsafeHelper.AddTypedOffset(ref brushesRef, i + 1), UnsafeHelper.AddTypedOffsetAsReadOnly(in nodesRef, i + 1));
                goto case 1;
            case 1:
                ApplyThemeBrush(provider, ref UnsafeHelper.AddTypedOffset(ref brushesRef, i), UnsafeHelper.AddTypedOffsetAsReadOnly(in nodesRef, i));
                break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ApplyThemeBrushesUnsafe(IThemeResourceProvider provider, D2D1Brush?[] brushes, string[] nodes, string nodePrefix, nuint length)
    {
        ref D2D1Brush? brushesRef = ref UnsafeHelper.GetArrayDataReference(brushes);
        ref readonly string nodesRef = ref UnsafeHelper.GetArrayDataReference(nodes);
        int i;
        for (i = 0; length >= 4; length -= 4, i += 4)
        {
            ApplyThemeBrush(provider, ref UnsafeHelper.AddTypedOffset(ref brushesRef, i), nodePrefix + "." + UnsafeHelper.AddTypedOffsetAsReadOnly(in nodesRef, i));
            ApplyThemeBrush(provider, ref UnsafeHelper.AddTypedOffset(ref brushesRef, i + 1), nodePrefix + "." + UnsafeHelper.AddTypedOffsetAsReadOnly(in nodesRef, i + 1));
            ApplyThemeBrush(provider, ref UnsafeHelper.AddTypedOffset(ref brushesRef, i + 2), nodePrefix + "." + UnsafeHelper.AddTypedOffsetAsReadOnly(in nodesRef, i + 2));
            ApplyThemeBrush(provider, ref UnsafeHelper.AddTypedOffset(ref brushesRef, i + 3), nodePrefix + "." + UnsafeHelper.AddTypedOffsetAsReadOnly(in nodesRef, i + 3));
        }
        switch (length)
        {
            case 3:
                ApplyThemeBrush(provider, ref UnsafeHelper.AddTypedOffset(ref brushesRef, i + 2), nodePrefix + "." + UnsafeHelper.AddTypedOffsetAsReadOnly(in nodesRef, i + 2));
                goto case 2;
            case 2:
                ApplyThemeBrush(provider, ref UnsafeHelper.AddTypedOffset(ref brushesRef, i + 1), nodePrefix + "." + UnsafeHelper.AddTypedOffsetAsReadOnly(in nodesRef, i + 1));
                goto case 1;
            case 1:
                ApplyThemeBrush(provider, ref UnsafeHelper.AddTypedOffset(ref brushesRef, i), nodePrefix + "." + UnsafeHelper.AddTypedOffsetAsReadOnly(in nodesRef, i));
                break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ApplyThemeBrush(IThemeResourceProvider provider, ref D2D1Brush? brushRef, string node)
        => DisposeHelper.SwapDispose(ref brushRef, provider.TryGetBrush(node, out D2D1Brush? result) ? result.Clone() : null);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ApplyThemeToElement(IThemeResourceProvider provider, UIElement? element)
        => element?.ApplyTheme(provider);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ApplyThemeToElements<TEnumerable>(IThemeResourceProvider provider, TEnumerable elements)
        where TEnumerable : IEnumerable<UIElement?>
    {
        using ArrayPool<UIElement?>.RentScope scope = ArrayPool<UIElement?>.Shared.EnterRentScopeAndCapture(elements);
        ApplyThemeToElementsCore(provider, in scope.GetReferenceOfFirstElement(), MathHelper.MakeUnsigned(scope.Count));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ApplyThemeToElementsUnsafe(IThemeResourceProvider provider, ref readonly UIElement? elementsRef,int count)
        => ApplyThemeToElementsCore(provider, in elementsRef, MathHelper.MakeUnsigned(count));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ApplyThemeToElementsCore(IThemeResourceProvider provider, ref readonly UIElement? elementArrayRef, nuint length)
    {
        int i;
        for (i = 0; length >= 4; length -= 4, i += 4)
        {
            ApplyThemeToElement(provider, UnsafeHelper.AddTypedOffsetAsReadOnly(in elementArrayRef, i));
            ApplyThemeToElement(provider, UnsafeHelper.AddTypedOffsetAsReadOnly(in elementArrayRef, i + 1));
            ApplyThemeToElement(provider, UnsafeHelper.AddTypedOffsetAsReadOnly(in elementArrayRef, i + 2));
            ApplyThemeToElement(provider, UnsafeHelper.AddTypedOffsetAsReadOnly(in elementArrayRef, i + 3));
        }
        switch (length)
        {
            case 3:
                ApplyThemeToElement(provider, UnsafeHelper.AddTypedOffsetAsReadOnly(in elementArrayRef, i + 2));
                goto case 2;
            case 2:
                ApplyThemeToElement(provider, UnsafeHelper.AddTypedOffsetAsReadOnly(in elementArrayRef, i + 1));
                goto case 1;
            case 1:
                ApplyThemeToElement(provider, UnsafeHelper.AddTypedOffsetAsReadOnly(in elementArrayRef, i));
                break;
        }
    }
}
