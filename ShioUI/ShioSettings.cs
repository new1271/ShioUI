using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using RiceTea.Core;

using ShioUI.Theme;

namespace ShioUI;

public delegate IEnumerable<KeyValuePair<string, IThemedColorFactory>> ThemedColorsBuildingHandler(
    Func<string, IThemedColorFactory> queryFunc);
public delegate IEnumerable<KeyValuePair<string, IThemedBrushFactory>> ThemedBrushesBuildingHandler(
    Func<string, IThemedColorFactory> queryColorFunc, Func<string, IThemedBrushFactory> queryBrushFunc);

public delegate void ThemeBuildingEventHandler(
    IList<ThemedColorsBuildingHandler> colorsHandlerList,
    IList<ThemedBrushesBuildingHandler> brushesHandlerList);

public static class ShioSettings
{
    public const string ReservedGpuName_None = "#none";
    public const string ReservedGpuName_Default = "#default";
    public const string ReservedGpuName_MinimumPower = "#default_minimum_power";
    public const string ReservedGpuName_HighPerformance = "#default_high_performance";

    private static WindowMaterial _windowMaterial = WindowMaterial.Default;
    private static ThemeBuildingEventHandler? _lightThemeBuildingHandler, _darkThemeBuildingHandler;

    public static bool UseDebugMode { get; set; } = RTCore.IsDebug;
    public static string TargetGpuName { get; set; } = ReservedGpuName_Default;

    public static event ThemeBuildingEventHandler? LightThemeBuilding
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        add => _lightThemeBuildingHandler += value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        remove => _lightThemeBuildingHandler -= value;
    }

    public static event ThemeBuildingEventHandler? DarkThemeBuilding
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        add => _darkThemeBuildingHandler += value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        remove => _darkThemeBuildingHandler -= value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void OnLightThemeBuilding(
        IList<ThemedColorsBuildingHandler> colorsBuildingHandlerList,
        IList<ThemedBrushesBuildingHandler> brushesBuildingHandlerList) 
        => _lightThemeBuildingHandler?.Invoke(colorsBuildingHandlerList, brushesBuildingHandlerList);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void OnDarkThemeBuilding(
        IList<ThemedColorsBuildingHandler> colorsBuildingHandlerList,
        IList<ThemedBrushesBuildingHandler> brushesBuildingHandlerList) 
        => _darkThemeBuildingHandler?.Invoke(colorsBuildingHandlerList, brushesBuildingHandlerList);

    public static WindowMaterial WindowMaterial
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _windowMaterial;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _windowMaterial = value;
    }
}
