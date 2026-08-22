using System.Drawing;
using System.Runtime.CompilerServices;

using ShioUI.Internals.Native;

namespace ShioUI.Utils;

public static class MouseHelper
{
    [SkipLocalsInit]
    public static unsafe Point GetMousePosition()
    {
        Point point;
        if (User32.GetCursorPos(&point))
            return point;
        return Point.Empty;
    }
}
