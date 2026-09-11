using System.Runtime.CompilerServices;

using InlineMethod;

using RiceTea.Core.Helpers;
using RiceTea.Core.Native;
using RiceTea.Core.Structures;

namespace ShioUI.Graphics.Native.Direct2D.Effects;

public unsafe class D2D1Effect : D2D1Properties
{
    protected new enum MethodTable
    {
        _Start = D2D1Properties.MethodTable._End,
        SetInput,
        SetInputCount,
        GetInput,
        GetInputCount,
        GetOutput,
        _End
    }

    public D2D1Effect() : base() { }

    public D2D1Effect(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

    public uint InputCount
    {
        get => GetInputCount();
        set => SetInputCount(value);
    }

    [Inline(InlineBehavior.Keep, export: true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetInput(uint index, D2D1Image input) => SetInput(index, input, invalidate: SysBool32.True);

    public void SetInput(uint index, D2D1Image input, SysBool32 invalidate)
    {
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.SetInput);
        ((delegate* unmanaged[Stdcall]<void*, uint, void*, SysBool32, void>)functionPointer)(nativePointer, index, input == null ? null : input.NativePointer, invalidate);
        AfterUnmanagedCall(input);
    }

    [Inline(InlineBehavior.Remove)]
    private void SetInputCount(uint inputCount)
    {
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.SetInputCount);
        int hr = ((delegate* unmanaged[Stdcall]<void*, uint, int>)functionPointer)(nativePointer, inputCount);
        AfterUnmanagedCall();
        ThrowHelper.ThrowExceptionForHR(hr);
    }

    public D2D1Image? GetInput(uint index)
    {
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetInput);
        ((delegate* unmanaged[Stdcall]<void*, uint, void*, void>)functionPointer)(nativePointer, index, &nativePointer);
        AfterUnmanagedCall();
        return nativePointer == null ? null : new D2D1Image(nativePointer, ReferenceType.Owned);
    }

    [Inline(InlineBehavior.Remove)]
    private uint GetInputCount()
    {
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetInputCount);
        uint result = ((delegate* unmanaged[Stdcall]<void*, uint>)functionPointer)(nativePointer);
        AfterUnmanagedCall();
        return result;
    }

    public D2D1Image? GetOutput()
    {
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetOutput);
        ((delegate* unmanaged[Stdcall]<void*, void*, void>)functionPointer)(nativePointer, &nativePointer);
        AfterUnmanagedCall();
        return nativePointer == null ? null : new D2D1Image(nativePointer, ReferenceType.Owned);
    }
}