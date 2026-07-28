using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

using InlineMethod;

using ShioUI.Internals;
using ShioUI.Internals.Native;
using ShioUI.Windows;

namespace ShioUI.Theme;

public delegate void ThemeChangingEventHandler(IThemeContext context);

public static class ThemeManager
{
    private static readonly Dictionary<string, IThemeContext> _themeDict = new Dictionary<string, IThemeContext>();
    private static readonly HashSet<IThemeProvider> _providers = new HashSet<IThemeProvider>();
    private static readonly Lock _lock = new Lock();

    private static IThemeContext? _currentTheme;

    public static event ThemeChangingEventHandler? ThemeChanging;
    public static event EventHandler? ThemeChanged;

    public static IThemeContext CurrentTheme
    {
        get
        {
            IThemeContext? result = _currentTheme;
            if (result is not null)
                return result;
            lock (_lock)
            {
                result = _currentTheme;
                if (result is not null)
                    return result;
                DefaultThemeProvider provider = GetDefaultThemeProvider();
                result = provider.LightTheme;
                _currentTheme = result;
                return result;
            }
        }
        set
        {
            lock (_lock)
            {
                if (ReferenceEquals(_currentTheme, value))
                    return;
                OnThemeChanging(value);
                _currentTheme = value;
                OnThemeChanged(value);
            }
        }
    }

    private static DefaultThemeProvider GetDefaultThemeProvider()
    {
        DefaultThemeProvider provider = DefaultThemeProvider.Instance;
        _providers.Add(provider);
        return provider;
    }

    public static void RegisterThemeProvider(IThemeProvider provider)
    {
        lock (_lock)
            _providers.Add(provider);
    }

    public static void UnregisterThemeProvider(IThemeProvider provider)
    {
        lock (_lock)
            _providers.Remove(provider);
    }

    [Inline(InlineBehavior.Remove)]
    private static void OnThemeChanging(IThemeContext context)
        => ThemeChanging?.Invoke(context);

    [Inline(InlineBehavior.Remove)]
    private static void OnThemeChanged(IThemeContext context)
    {
        ThemeChanged?.Invoke(null, EventArgs.Empty);
        CoreWindow.NotifyThemeChanged(context);
        UpdateDarkModeState(context);
    }

    [Inline(InlineBehavior.Remove)]
    private static void UpdateDarkModeState(IThemeContext context)
    {
        if (SystemConstants.VersionLevel >= SystemVersionLevel.Windows_10_19H1)
            UxTheme.SetPreferredAppMode(context.IsDarkTheme ? PreferredAppMode.ForceDark : PreferredAppMode.ForceLight);
    }

    public static bool TryGetThemeContext(string themeId, [NotNullWhen(true)] out IThemeContext? theme)
    {
        Dictionary<string, IThemeContext> themeDict = _themeDict;
        lock (_lock)
        {
            if (themeDict.TryGetValue(themeId, out theme))
                return true;
            foreach (IThemeProvider provider in _providers)
            {
                if (provider.TryGetTheme(themeId, out theme))
                {
                    themeDict.Add(themeId, theme);
                    return true;
                }
            }
            switch (themeId)
            {
                case DefaultThemeProvider.LightThemeId:
                    theme = GetDefaultThemeProvider().LightTheme;
                    return true;
                case DefaultThemeProvider.DarkThemeId:
                    theme = GetDefaultThemeProvider().DarkTheme;
                    return true;
            }
        }
        return false;
    }
}
