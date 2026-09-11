using System;
using System.Collections;
using System.Collections.Generic;

using InlineMethod;

using RiceTea.Core.Helpers;
using RiceTea.Core.Native;
using RiceTea.Core.Threading;

namespace ShioUI.Graphics.Native.Direct2D.Brushes;

/// <summary>
/// Represents an collection of gradient stops that can then be the source resource
/// for either a linear or radial gradient brush.
/// </summary>
public unsafe sealed class D2D1GradientStopCollection : D2D1Resource, IReadOnlyCollection<D2D1GradientStop>
{
    private new enum MethodTable
    {
        _Start = D2D1Resource.MethodTable._End,
        GetGradientStopCount = _Start,
        GetGradientStops,
        GetColorInterpolationGamma,
        GetExtendMode,
        _End
    }

    private readonly LazyTiny<D2D1GradientStop[], D2D1GradientStopCollection> _arrayLazy;

    public D2D1GradientStopCollection() : base()
    {
        _arrayLazy = new LazyTiny<D2D1GradientStop[], D2D1GradientStopCollection>(_this => _this.GetGradientStops(GetGradientStopCount()), this);
    }

    public D2D1GradientStopCollection(void* nativePointer, ReferenceType referenceType) : base(nativePointer, referenceType)
    {
        _arrayLazy = new LazyTiny<D2D1GradientStop[], D2D1GradientStopCollection>(_this => _this.GetGradientStops(GetGradientStopCount()), this);
    }

    /// <summary>
    /// The number of stops in the gradient.
    /// </summary>
    public uint Count => GetGradientStopCount();

    int IReadOnlyCollection<D2D1GradientStop>.Count => MathHelper.MakeSigned(GetGradientStopCount());

    public D2D1GradientStop this[int index] => index < 0 ?
        ArgumentOutOfRangeException.Throw<D2D1GradientStop>(nameof(index)) :
        _arrayLazy.Value[index];

    public D2D1GradientStop this[uint index] => _arrayLazy.Value[index];

    public Enumerator GetEnumerator() => new Enumerator(_arrayLazy.Value);

    IEnumerator<D2D1GradientStop> IEnumerable<D2D1GradientStop>.GetEnumerator() => GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _arrayLazy.Value.GetEnumerator();

    public D2D1GradientStop[] ToArray()
    {
        D2D1GradientStop[] array = _arrayLazy.Value;
        int length = array.Length;
        if (length <= 0)
            return Array.Empty<D2D1GradientStop>();
        D2D1GradientStop[] result = new D2D1GradientStop[length];
        Array.Copy(array, result, length);
        return result;
    }

    /// <summary>
    /// Returns whether the interpolation occurs with 1.0 or 2.2 gamma.
    /// </summary>
    public D2D1Gamma ColorInterpolationGamma => GetColorInterpolationGamma();

    public D2D1ExtendMode ExtendMode => GetExtendMode();

    [Inline(InlineBehavior.Remove)]
    private uint GetGradientStopCount()
    {
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetGradientStopCount);
        uint result = ((delegate* unmanaged[Stdcall]<void*, uint>)functionPointer)(nativePointer);
        AfterUnmanagedCall();
        return result;
    }

    [Inline(InlineBehavior.Remove)]
    private D2D1GradientStop[] GetGradientStops(uint gradientStopsCount)
    {
        D2D1GradientStop[] result = new D2D1GradientStop[gradientStopsCount];
        fixed (D2D1GradientStop* gradientStops = result)
            GetGradientStops(gradientStops, gradientStopsCount);
        return result;
    }

    [Inline(InlineBehavior.Remove)]
    private void GetGradientStops(D2D1GradientStop* gradientStops, uint gradientStopsCount)
    {
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetGradientStops);
        ((delegate* unmanaged[Stdcall]<void*, D2D1GradientStop*, uint, void>)functionPointer)(nativePointer, gradientStops, gradientStopsCount);
        AfterUnmanagedCall();
    }

    [Inline(InlineBehavior.Remove)]
    private D2D1Gamma GetColorInterpolationGamma()
    {
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetColorInterpolationGamma);
        D2D1Gamma result = ((delegate* unmanaged[Stdcall]<void*, D2D1Gamma>)functionPointer)(nativePointer);
        AfterUnmanagedCall();
        return result;
    }

    private D2D1ExtendMode GetExtendMode()
    {
        void* nativePointer = NativePointer;
        void* functionPointer = GetFunctionPointerOrThrow(nativePointer, (int)MethodTable.GetExtendMode);
        D2D1ExtendMode result = ((delegate* unmanaged[Stdcall]<void*, D2D1ExtendMode>)functionPointer)(nativePointer);
        AfterUnmanagedCall();
        return result;
    }

    public struct Enumerator : IEnumerator<D2D1GradientStop>
    {
        private readonly int _bound;
        private readonly D2D1GradientStop[] _array;

        private int _index;

        public Enumerator(D2D1GradientStop[] array)
        {
            _array = array;
            _bound = array.Length;
            _index = -1;
        }

        public readonly D2D1GradientStop Current => _index < 0 || _index >= _bound ? default : _array[_index];

        readonly object IEnumerator.Current => _index < 0 || _index >= _bound ? default : _array[_index];

        public bool MoveNext()
        {
            if (_index < _bound)
            {
                _index++;
                return true;
            }
            return false;
        }

        public void Reset()
        {
            _index = -1;
        }

        public void Dispose() => Reset();
    }
}