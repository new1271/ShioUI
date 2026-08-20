using System.Numerics;

using RiceTea.Core.Structures;

namespace ShioUI.Traits;

public readonly record struct DpiChangedEventArgs(PointU Dpi, Vector2 PointsPerPixel, Vector2 PixelsPerPoint);

public interface IDpiChangedHandler
{
    void OnDpiChanged(in DpiChangedEventArgs args);
}
