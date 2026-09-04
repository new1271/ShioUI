using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ShioUI.Utils;

partial class Keys
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<bool> IsControlPressedAsync() => IsKeyPressedAsync(VirtualKey.Control);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<bool> IsLeftControlPressedAsync() => IsKeyPressedAsync(VirtualKey.LeftControl);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<bool> IsRightControlPressedAsync() => IsKeyPressedAsync(VirtualKey.RightControl);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<bool> IsShiftPressedAsync() => IsKeyPressedAsync(VirtualKey.Shift);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<bool> IsLeftShiftPressedAsync() => IsKeyPressedAsync(VirtualKey.LeftShift);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<bool> IsRightShiftPressedAsync() => IsKeyPressedAsync(VirtualKey.RightShift);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<bool> IsAltPressedAsync() => IsKeyPressedAsync(VirtualKey.Alt);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<bool> IsLeftAltPressedAsync() => IsKeyPressedAsync(VirtualKey.LeftAlt);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<bool> IsRightAltPressedAsync() => IsKeyPressedAsync(VirtualKey.RightAlt);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<bool> IsNumLockToggledAsync() => IsKeyToggledAsync(VirtualKey.NumLock);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<bool> IsCapsLockToggledAsync() => IsKeyToggledAsync(VirtualKey.CapsLock);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<bool> IsScrollLockToggledAsync() => IsKeyToggledAsync(VirtualKey.ScrollLock);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<bool> IsKeyPressedAsync(VirtualKey key)
        => WindowMessageLoop.InvokeTaskAsync(IsKeyPressedCore, key);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<bool> IsKeyToggledAsync(VirtualKey key)
        => WindowMessageLoop.InvokeTaskAsync(IsKeyToggledCore, key);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<(bool isPressed, bool isToggled)> GetKeyStateAsync(VirtualKey key)
        => WindowMessageLoop.InvokeTaskAsync(GetKeyStateCore, key);
}
