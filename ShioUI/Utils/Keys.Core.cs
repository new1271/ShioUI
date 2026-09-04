using System.Runtime.CompilerServices;

using ShioUI.Internals.Native;

namespace ShioUI.Utils;

partial class Keys
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsKeyPressedCore(VirtualKey key) => User32.GetKeyState(key) < 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsKeyToggledCore(VirtualKey key) => (User32.GetKeyState(key) & 0b01) == 0b01;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static (bool isPressed, bool isToggled) GetKeyStateCore(VirtualKey key)
    {
        short result = User32.GetKeyState(key);
        return (isPressed: result < 0,
            isToggled: (result & 0b01) == 0b01);
    }
}
