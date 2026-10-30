using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using ShioUI.Graphics.Native.Direct2D;
using ShioUI.Graphics.Native.Direct2D.Brushes;

namespace ShioUI.Theme;

public interface IThemedBrushFactory
{
    D2D1Brush CreateDefaultBrush(D2D1DeviceContext context);

    D2D1Brush CreateBrushByMaterial(D2D1DeviceContext context, WindowMaterial material);

    IEnumerable<WindowMaterial> GetVariants();
}

public static partial class ThemedBrushFactory
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IThemedBrushFactory FromColor(in D2D1ColorF color) => new SolidColorImpl(color);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IThemedBrushFactory FromColorFactory(IThemedColorFactory factory) => new Adapter(factory);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IThemedBrushFactory AmplifiedFrom(IThemedBrushFactory factory, float amplifier) => new AmplifiedImpl(factory, amplifier);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IThemedBrushFactory FromFunction(Func<D2D1DeviceContext, WindowMaterial, D2D1Brush> function) => new FunctionImpl(function);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Builder CreateBuilder(in D2D1ColorF baseBrushColor) => new Builder(new SolidColorImpl(baseBrushColor));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Builder CreateBuilder(Func<D2D1DeviceContext, D2D1Brush> baseBrushFactory) => new Builder(new SimpleImpl(baseBrushFactory));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Builder CreateBuilder(IThemedBrushFactory baseFactory) => new Builder(baseFactory);
}
