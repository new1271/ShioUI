using System.Runtime.CompilerServices;

namespace ShioUI.Utils;

public static partial class Keys
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsControlPressed() => IsKeyPressed(VirtualKey.Control);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsLeftControlPressed() => IsKeyPressed(VirtualKey.LeftControl);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsRightControlPressed() => IsKeyPressed(VirtualKey.RightControl);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsShiftPressed() => IsKeyPressed(VirtualKey.Shift);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsLeftShiftPressed() => IsKeyPressed(VirtualKey.LeftShift);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsRightShiftPressed() => IsKeyPressed(VirtualKey.RightShift);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAltPressed() => IsKeyPressed(VirtualKey.Alt);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsLeftAltPressed() => IsKeyPressed(VirtualKey.LeftAlt);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsRightAltPressed() => IsKeyPressed(VirtualKey.RightAlt);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNumLockToggled() => IsKeyToggled(VirtualKey.NumLock);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsCapsLockToggled() => IsKeyToggled(VirtualKey.CapsLock);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsScrollLockToggled() => IsKeyToggled(VirtualKey.ScrollLock);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsKeyPressed(VirtualKey key)
        => WindowMessageLoop.Invoke(IsKeyPressedCore, key);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsKeyToggled(VirtualKey key)
        => WindowMessageLoop.Invoke(IsKeyToggledCore, key);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (bool isPressed, bool isToggled) GetKeyState(VirtualKey key)
        => WindowMessageLoop.Invoke(GetKeyStateCore, key);
}
