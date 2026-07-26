using System;

using RiceTea.Core;
using RiceTea.Core.Helpers;

namespace ShioUI.Controls;

partial class Placeholder : UIElement
{
    public bool HasBackground
    {
        get => Atomics.Read(ref _hasBackground) != 0;
        set
        {
            nuint rawValue = MathHelper.BooleanToNativeUnsigned(value);
            if (Atomics.Exchange(ref _hasBackground, rawValue) == rawValue)
                return;
            if (value && StringHelper.IsNullOrEmpty(ThemePrefix))
                InvalidOperationException.Throw();
            Update();
        }
    }
}
