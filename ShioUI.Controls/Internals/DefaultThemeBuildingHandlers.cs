using System;
using System.Collections.Generic;

using ShioUI.Theme;

namespace ShioUI.Controls.Internals;

internal static partial class ThemeBuildingHandlers
{
    public static void HookEvents()
    {
        ShioSettings.LightThemeBuilding += LightTheme.HookEvent;
        ShioSettings.DarkThemeBuilding += DarkTheme.HookEvent;
    }

    public static partial class LightTheme
    {
        public static void HookEvent(
            IList<ThemedColorsBuildingHandler> colorsBuildingFuncList,
            IList<ThemedBrushesBuildingHandler> brushesBuildingFuncList)
            => brushesBuildingFuncList.Add(InitializeBrushFactories);

        private static partial IEnumerable<KeyValuePair<string, IThemedBrushFactory>> InitializeBrushFactories(
                Func<string, IThemedColorFactory> queryColorFunc, Func<string, IThemedBrushFactory> queryBrushFunc);
    }

    public static partial class DarkTheme
    {
        public static void HookEvent(
            IList<ThemedColorsBuildingHandler> colorsBuildingFuncList,
            IList<ThemedBrushesBuildingHandler> brushesBuildingFuncList)
            => brushesBuildingFuncList.Add(InitializeBrushFactories);

        private static partial IEnumerable<KeyValuePair<string, IThemedBrushFactory>> InitializeBrushFactories(
                Func<string, IThemedColorFactory> queryColorFunc, Func<string, IThemedBrushFactory> queryBrushFunc);
    }
}
