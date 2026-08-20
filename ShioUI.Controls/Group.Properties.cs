using System;
using System.Runtime.CompilerServices;

using RiceTea.Core;

using RiceTea.Core.Helpers;

using ShioUI.Controls.Traits;
using ShioUI.Utils;

namespace ShioUI.Controls;

partial class Group : UIElement, IAppendableElementContainer
{
    public UIElement? FirstChild
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _children.FirstOrDefault();
    }

    public UIElement? LastChild
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _children.LastOrDefault();
    }

    public UIElementCollection Children
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _children;
    }

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
