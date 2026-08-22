using System.Runtime.CompilerServices;

using RiceTea.Core;

namespace ShioUI.Controls;

partial class ProgressBar
{
    public double Value
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Atomics.Read(ref _value);
        set
        {
            if (Atomics.Exchange(ref _value, value) == value)
                return;
            Update();
        }
    }

    public double Maximum
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Atomics.Read(ref _maximum);
        set
        {
            if (Atomics.Exchange(ref _maximum, value) == value)
                return;
            Update();
        }
    }

}
