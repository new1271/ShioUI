using System.Runtime.CompilerServices;

using RiceTea.Core;
using RiceTea.Core.Helpers;

namespace ShioUI.Controls;

partial class ButtonBase
{
    public event MouseNotifyEventHandler? Click;

    protected ButtonTriState PressState
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (ButtonTriState)Atomics.Read(ref _pressState);
    }

    public bool Enabled
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => MathHelper.ToBoolean(Atomics.Read(ref _enabled));
        set
        {
            uint rawValue = MathHelper.BooleanToUInt32(value);
            if (Atomics.Exchange(ref _enabled, rawValue) == rawValue)
                return;
            Update();
        }
    }
}
