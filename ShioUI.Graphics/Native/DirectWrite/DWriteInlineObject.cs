using System.Runtime.CompilerServices;
using System.Security;

using RiceTea.Core.Helpers;
using RiceTea.Core.Native;
using RiceTea.Core.Windows.ObjectModels;

namespace ShioUI.Graphics.Native.DirectWrite;

/// <summary>
/// The <see cref="DWriteInlineObject"/> wraps an application defined inline graphic,
/// allowing DWrite to query metrics as if it was a glyph inline with the text.
/// </summary>
[SuppressUnmanagedCodeSecurity]
public unsafe sealed class DWriteInlineObject : ComObject
{
    private new enum MethodTable
    {
        _Start = ComObject.MethodTable._End,
        Draw = _Start,
        GetMetrics,
        GetOverhangMetrics,
        GetBreakConditions,
        _End
    }

    public DWriteInlineObject() : base() { }

    public DWriteInlineObject(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType) { }

    /// <summary>
    /// TextLayout calls this callback function to get the measurement of the inline object.
    /// </summary>
    [SkipLocalsInit]
    public DWriteInlineObjectMetrics GetMetrics()
    {
        DWriteInlineObjectMetrics metrics;
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetMetrics);
        int hr = ((delegate* unmanaged[Stdcall]<void*, DWriteInlineObjectMetrics*, int>)functionPointer)(nativePointer, &metrics);
        ThrowHelper.ThrowExceptionForHR(hr);
        return metrics;
    }

    /// <summary>
    /// TextLayout calls this callback function to get the visible extents (in DIPs) of the inline object. <br/>
    /// In the case of a simple bitmap, with no padding and no overhang, all the overhangs will
    /// simply be zeroes.
    /// </summary>
    /// <returns>
    /// Overshoot of visible extents (in DIPs) outside the object.
    /// </returns>
    /// <remarks>
    /// The overhangs should be returned relative to the reported size of the object (<see cref="DWriteInlineObjectMetrics.Width"/> / <see cref="DWriteInlineObjectMetrics.Height"/>), <br/>
    /// and should not be baseline adjusted. <br/>
    /// If you have an image that is actually 100x100 DIPs, but you want it slightly inset (perhaps it has a glow) by 20 DIPs on each side, <br/>
    /// you would return a width/height of 60x60 and four overhangs of 20 DIPs.
    /// </remarks>
    [SkipLocalsInit]
    public DWriteOverhangMetrics GetOverhangMetrics()
    {
        DWriteOverhangMetrics metrics;
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetOverhangMetrics);
        int hr = ((delegate* unmanaged[Stdcall]<void*, DWriteOverhangMetrics*, int>)functionPointer)(nativePointer, &metrics);
        ThrowHelper.ThrowExceptionForHR(hr);
        return metrics;
    }

    /// <summary>
    /// Layout uses this to determine the line breaking behavior of the inline object
    /// amidst the text.
    /// </summary>
    /// <returns>
    /// Line-breaking conditions. <br/>
    /// Before is for the content immediately preceding it. <br/>
    /// After is for the content immediately following it. <br/>
    /// </returns>
    [SkipLocalsInit]
    public (DWriteBreakCondition Before, DWriteBreakCondition After) GetBreakConditions()
    {
        DWriteBreakCondition before, after;
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetBreakConditions);
        int hr = ((delegate* unmanaged[Stdcall]<void*, DWriteBreakCondition*, DWriteBreakCondition*, int>)functionPointer)(nativePointer, &before, &after);
        ThrowHelper.ThrowExceptionForHR(hr);
        return (before, after);
    }
}