using System.Diagnostics.CodeAnalysis;

using RiceTea.Core.Extensions;
using RiceTea.Core.Threading;

namespace ShioUI.Theme;

public sealed partial class DefaultThemeProvider : IThemeProvider
{
    public const string LightThemeId = "#light";
    public const string DarkThemeId = "#dark";
    
    private static readonly LazyTiny<DefaultThemeProvider> _instanceLazy = new(() => new DefaultThemeProvider(), isThreadSafe: true);

    public static DefaultThemeProvider Instance => _instanceLazy.Value;

    private readonly LightThemeContext _lightTheme = new LightThemeContext();
    private readonly DarkThemeContext _darkTheme = new DarkThemeContext();

    public IThemeContext LightTheme => _lightTheme;
    public IThemeContext DarkTheme => _darkTheme;

    private DefaultThemeProvider() { }

    bool IThemeProvider.TryGetTheme(string themeId, [NotNullWhen(true)] out IThemeContext? theme)
    {
        theme = themeId.ToLowerAscii() switch
        {
            LightThemeId => _lightTheme,
            DarkThemeId => _darkTheme,
            _ => null
        };
        return theme is not null;
    }
}
