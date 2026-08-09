using System.Runtime.CompilerServices;

using RiceTea.Core;

using ShioUI.Utils;

namespace ShioUI.Controls;

partial class FontIconButton
{
    public FontIcon? Icon
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Atomics.Read(ref _icon);
        set
        {
            if (ReferenceEquals(Atomics.Exchange(ref _icon, value), value))
                return;
            Update();
        }
    }
}
