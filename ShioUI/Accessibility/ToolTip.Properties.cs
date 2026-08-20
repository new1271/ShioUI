using System.Runtime.CompilerServices;

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
}
