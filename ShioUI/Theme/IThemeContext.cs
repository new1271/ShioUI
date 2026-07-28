using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ShioUI.Theme;

public interface IThemeContext
{
    bool IsDarkTheme { get; }

    string FontName { get; set; }

    IThemeContext Clone();

    bool TryGetColorFactory(string node, [NotNullWhen(true)] out IThemedColorFactory? colorFactory);

    bool TryGetBrushFactory(string node, [NotNullWhen(true)] out IThemedBrushFactory? brushFactory);

    IEnumerable<KeyValuePair<string, IThemedColorFactory>> EnumerateColorFactories();

    IEnumerable<KeyValuePair<string, IThemedBrushFactory>> EnumerateBrushFactories();

    IEnumerable<KeyValuePair<string, IThemedColorFactory>> BuildColorFactories(Func<string, IThemedColorFactory> queryFunc);

    IEnumerable<KeyValuePair<string, IThemedBrushFactory>> BuildBrushFactories(
        Func<string, IThemedColorFactory> queryColorFunc, Func<string, IThemedBrushFactory> queryBrushFunc);
}
