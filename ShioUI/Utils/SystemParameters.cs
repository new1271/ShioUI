using RiceTea.Core.Helpers;

using ShioUI.Internals.Native;

namespace ShioUI.Utils;

public static class SystemParameters
{
    private const float SpeedInterval = (400f - 33f) / 31f;

    public static readonly uint KeyboardDelay;
    public static readonly uint KeyboardSpeed;
    public static readonly uint DoubleClickTime;
    public static readonly uint MouseHoverTime;
    public static readonly uint MouseHoverWidth;
    public static readonly uint MouseHoverHeight;

    static unsafe SystemParameters()
    {
        const uint SPI_GETKEYBOARDDELAY = 0x0016;
        const uint SPI_GETKEYBOARDSPEED = 0x000A;
        const uint SPI_GETMOUSEHOVERTIME = 0x0066;
        const uint SPI_GETMOUSEHOVERWIDTH = 0x0062;
        const uint SPI_GETMOUSEHOVERHEIGHT = 0x0063;

        nuint val = default;
        KeyboardDelay = User32.SystemParametersInfoW(SPI_GETKEYBOARDDELAY, 0, &val, 0) ? ((uint)val + 1) * 250 : 500;
        KeyboardSpeed = User32.SystemParametersInfoW(SPI_GETKEYBOARDSPEED, 0, &val, 0) ? val switch
        {
            0 => 400,
            31 => 33,
            _ => (uint)MathHelper.Max(400f - val * SpeedInterval, 0.0f),
        } : 400;
        DoubleClickTime = User32.GetDoubleClickTime();
        MouseHoverTime = User32.SystemParametersInfoW(SPI_GETMOUSEHOVERTIME, 0, &val, 0) ? (uint)val : 400;
        MouseHoverWidth = User32.SystemParametersInfoW(SPI_GETMOUSEHOVERWIDTH, 0, &val, 0) ? (uint)val : 2;
        MouseHoverHeight = User32.SystemParametersInfoW(SPI_GETMOUSEHOVERHEIGHT, 0, &val, 0) ? (uint)val : 2;
    }
}
