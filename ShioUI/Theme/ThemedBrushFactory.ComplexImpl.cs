using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using RiceTea.Core.Extensions;
using RiceTea.Core.Helpers;

using ShioUI.Graphics.Native.Direct2D;
using ShioUI.Graphics.Native.Direct2D.Brushes;

namespace ShioUI.Theme;

partial class ThemedBrushFactory
{
    private sealed class ComplexImpl : IThemedBrushFactory
    {
        private readonly IThemedBrushFactory _base;
        private readonly byte[] _overrideKeys;
        private readonly Func<D2D1DeviceContext, D2D1Brush>[] _overrideBrushFactories;

        internal ComplexImpl(ComplexImpl original)
        {
            _base = original._base;
            _overrideKeys = original._overrideKeys;
            _overrideBrushFactories = original._overrideBrushFactories;
        }

        internal ComplexImpl(IThemedBrushFactory baseBrushFactory, byte[] overrideKeys,
            Func<D2D1DeviceContext, D2D1Brush>[] overrideBrushFactories)
        {
            if (overrideKeys.Length != overrideBrushFactories.Length)
                throw new InvalidOperationException();
            _base = baseBrushFactory;
            _overrideKeys = overrideKeys;
            _overrideBrushFactories = overrideBrushFactories;
        }

        public D2D1Brush CreateDefaultBrush(D2D1DeviceContext context)
        {
            int index = SequenceHelper.IndexOf(_overrideKeys, (byte)WindowMaterial.Default);
            if (index < 0)
                return _base.CreateDefaultBrush(context);

            return _overrideBrushFactories[index].Invoke(context);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public D2D1Brush CreateBrushByMaterial(D2D1DeviceContext context, WindowMaterial material)
        {
            int index = SequenceHelper.IndexOf(_overrideKeys, (byte)material);
            if (index < 0)
                return _base.CreateBrushByMaterial(context, material);

            return _overrideBrushFactories[index].Invoke(context);
        }

        public IEnumerable<WindowMaterial> GetVariants() 
            => _overrideKeys.Select(static val => (WindowMaterial)val).ConcatOptimized(_base.GetVariants());

        internal IThemedBrushFactory Destruct(out byte[] variantKeys, out Func<D2D1DeviceContext, D2D1Brush>[] variantBrushFactories)
        {
            variantKeys = _overrideKeys;
            variantBrushFactories = _overrideBrushFactories;
            return _base;
        }
    }
}
