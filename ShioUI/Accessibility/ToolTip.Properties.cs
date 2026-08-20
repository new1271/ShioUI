using System;
using System.Runtime.CompilerServices;

using RiceTea.Core;

namespace ShioUI.Accessibility;

partial class ToolTip
{
    public string ThemePrefix
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _themePrefix;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        init => _themePrefix = value;
    }

    public int ShowDelay
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Atomics.Read(ref _showDelay);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            Atomics.Write(ref _showDelay, value);
        }
    }
}
