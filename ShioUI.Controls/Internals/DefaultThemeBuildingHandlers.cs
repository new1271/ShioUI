using System;
using System.Collections.Generic;

using RiceTea.Core.Buffers;

using ShioUI.Theme;

namespace ShioUI.Controls.Internals;

internal static partial class DefaultThemeBuildingHandlers
{
    public static partial class LightTheme
    {
        public static void HookEvent(
            PooledList<DefaultThemeColorsBuildingFunction> colorsBuildingFuncList,
            PooledList<DefaultThemeBrushesBuildingFunction> brushesBuildingFuncList)
            => brushesBuildingFuncList.Add(InitializeBrushFactories);

        private static partial IEnumerable<KeyValuePair<string, IThemedBrushFactory>> InitializeBrushFactories(
                Func<string, IThemedColorFactory> queryColorFunc, Func<string, IThemedBrushFactory> queryBrushFunc);
    }

    public static partial class DarkTheme
    {
        public static void HookEvent(
            PooledList<DefaultThemeColorsBuildingFunction> colorsBuildingFuncList,
            PooledList<DefaultThemeBrushesBuildingFunction> brushesBuildingFuncList)
            => brushesBuildingFuncList.Add(InitializeBrushFactories);

        private static partial IEnumerable<KeyValuePair<string, IThemedBrushFactory>> InitializeBrushFactories(
                Func<string, IThemedColorFactory> queryColorFunc, Func<string, IThemedBrushFactory> queryBrushFunc);
    }
}
