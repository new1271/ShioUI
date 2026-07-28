using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using RiceTea.Core;
using RiceTea.Core.Buffers;

using ShioUI.Theme;

namespace ShioUI;

public delegate IEnumerable<KeyValuePair<string, IThemedColorFactory>> DefaultThemeColorsBuildingFunction(
    Func<string, IThemedColorFactory> queryFunc);
public delegate IEnumerable<KeyValuePair<string, IThemedBrushFactory>> DefaultThemeBrushesBuildingFunction(
    Func<string, IThemedColorFactory> queryColorFunc, Func<string, IThemedBrushFactory> queryBrushFunc);

public delegate void DefaultThemeBuildingEventHandler(
    PooledList<DefaultThemeColorsBuildingFunction> colorsBuildingFuncList,
    PooledList<DefaultThemeBrushesBuildingFunction> brushesBuildingFuncList);

public static class ShioSettings
{
    public const string ReservedGpuName_None = "#none";
    public const string ReservedGpuName_Default = "#default";
    public const string ReservedGpuName_MinimumPower = "#default_minimum_power";
    public const string ReservedGpuName_HighPerformance = "#default_high_performance";

    private static WindowMaterial _windowMaterial = WindowMaterial.Default;
    internal static DefaultThemeBuildingEventHandler? _lightThemeBuildingHandler, _darkThemeBuildingHandler;

    public static bool UseDebugMode { get; set; } = RTCore.IsDebug;
    public static string TargetGpuName { get; set; } = ReservedGpuName_Default;

    public static event DefaultThemeBuildingEventHandler? DefaultLightThemeBuilding
    {
        add => _lightThemeBuildingHandler += value;
        remove => _lightThemeBuildingHandler -= value;
    }

    public static event DefaultThemeBuildingEventHandler? DefaultDarkThemeBuilding
    {
        add => _darkThemeBuildingHandler += value;
        remove => _darkThemeBuildingHandler -= value;
    }

    public static WindowMaterial WindowMaterial
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _windowMaterial;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _windowMaterial = value;
    }
}
