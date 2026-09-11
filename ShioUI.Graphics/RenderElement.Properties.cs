using System.Runtime.CompilerServices;

using RiceTea.Core;

namespace ShioUI.Graphics;

partial class RenderElement
{
    public bool IsDisposed
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Atomics.Read(ref _disposed) != 0;
    }

    public int ElementId
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _identifier;
    }

    protected bool EnablePartialRendering
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _enablePartialRendering;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        init => _enablePartialRendering = value;
    }

    public bool IsRenderedOnce
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => CheckIsRenderedOnce(Atomics.Read(ref _needRefresh));
    }
}
