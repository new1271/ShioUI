using ShioUI.Controls.Internals;

namespace ShioUI.Controls;

public static class ShioControls
{
    public static void Initialize()
    {
        ShioSettings.DefaultLightThemeBuilding += DefaultThemeBuildingHandlers.LightTheme.HookEvent;
        ShioSettings.DefaultDarkThemeBuilding += DefaultThemeBuildingHandlers.DarkTheme.HookEvent;
    }
}
