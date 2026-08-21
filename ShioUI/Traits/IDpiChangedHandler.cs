using System.Numerics;

using RiceTea.Core.Structures;

namespace ShioUI.Traits;

public readonly record struct DpiChangedEventArgs(PointU Dpi, Vector2 DpiScaleFactorInversed, Vector2 DpiScaleFactor);

public interface IDpiChangedHandler
{
    void OnDpiChanged(in DpiChangedEventArgs args);
}
