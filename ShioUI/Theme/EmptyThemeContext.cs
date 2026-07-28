using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ShioUI.Theme;

public sealed class EmptyThemeContext : IThemeContext
{
    public static readonly EmptyThemeContext Instance = new EmptyThemeContext();

    private EmptyThemeContext() { }

    public string FontName
    {
        get => string.Empty;
        set { }
    }

    public bool IsDarkTheme => false;

    public IThemeContext Clone() => this;

    public IEnumerable<KeyValuePair<string, IThemedBrushFactory>> EnumerateBrushFactories() => Array.Empty<KeyValuePair<string, IThemedBrushFactory>>();

    public IEnumerable<KeyValuePair<string, IThemedColorFactory>> EnumerateColorFactories() => Array.Empty<KeyValuePair<string, IThemedColorFactory>>();

    public IEnumerable<KeyValuePair<string, IThemedBrushFactory>> BuildBrushFactories(
        Func<string, IThemedColorFactory> queryColorFunc, Func<string, IThemedBrushFactory> queryBrushFunc)
        => Array.Empty<KeyValuePair<string, IThemedBrushFactory>>();

    public IEnumerable<KeyValuePair<string, IThemedColorFactory>> BuildColorFactories(Func<string, IThemedColorFactory> queryFunc) 
        => Array.Empty<KeyValuePair<string, IThemedColorFactory>>();

    public bool TryGetBrushFactory(string node, [NotNullWhen(true)] out IThemedBrushFactory? brushFactory)
    {
        brushFactory = null;
        return false;
    }

    public bool TryGetColorFactory(string node, [NotNullWhen(true)] out IThemedColorFactory? colorFactory)
    {
        colorFactory = null;
        return false;
    }

    public bool TrySetBrushFactory(string node, IThemedBrushFactory brushFactory, bool overrides)
    {
        return false;
    }

    public bool TrySetColorFactory(string node, IThemedColorFactory colorFactory, bool overrides)
    {
        return false;
    }
}
