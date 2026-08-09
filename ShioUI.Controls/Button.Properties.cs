using System.Runtime.CompilerServices;

using RiceTea.Core;
using RiceTea.Core.Extensions;
using RiceTea.Core.Helpers;

using ShioUI.Layout;

namespace ShioUI.Controls;

partial class Button : IAutoWidthElement, IAutoHeightElement
{
    public float FontSize
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Atomics.Read(ref _fontSize);
        set
        {
            if (Atomics.Exchange(ref _fontSize, value) == value)
                return;
            DisposeHelper.SwapDisposeAtomic(ref _layout);
            Update(RenderObjectUpdateFlags.Format);
        }
    }

    public string Text
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Atomics.Read(ref _text);
        set
        {
            if (ReferenceEquals(Atomics.Exchange(ref _text, value), value))
                return;
            Update(RenderObjectUpdateFlags.Layout);
        }
    }

    public LayoutNode AutoWidthDefinition
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _autoLayoutDefinitions.AsUnsafeRef()[0] ??= new AutoWidthNode(GetWeakReference());
    }

    public LayoutNode AutoHeightDefinition
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _autoLayoutDefinitions.AsUnsafeRef()[1] ??= new AutoHeightNode(GetWeakReference());
    }
}
