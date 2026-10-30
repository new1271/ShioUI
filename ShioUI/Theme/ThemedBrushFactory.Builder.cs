using System;
using System.Runtime.CompilerServices;

using RiceTea.Core.Collections;

using ShioUI.Graphics.Native.Direct2D;
using ShioUI.Graphics.Native.Direct2D.Brushes;

namespace ShioUI.Theme;

partial class ThemedBrushFactory
{
    public sealed class Builder : ICloneable
    {
        private readonly UnwrappableList<byte> _overrideKeyList;
        private readonly UnwrappableList<Func<D2D1DeviceContext, D2D1Brush>> _overrideBrushFactoryList;
        private readonly IThemedBrushFactory _base;

        internal Builder(IThemedBrushFactory baseFactory)
        {
            _base = baseFactory;
            _overrideKeyList = new UnwrappableList<byte>(capacity: 0);
            _overrideBrushFactoryList = new UnwrappableList<Func<D2D1DeviceContext, D2D1Brush>>(capacity: 0);
        }

        private Builder(Builder original)
        {
            _base = original._base;
            _overrideKeyList = new UnwrappableList<byte>(original._overrideKeyList);
            _overrideBrushFactoryList = new UnwrappableList<Func<D2D1DeviceContext, D2D1Brush>>(original._overrideBrushFactoryList);
        }

        public Func<D2D1DeviceContext, D2D1Brush> this[WindowMaterial material]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                int index = _overrideKeyList.IndexOf((byte)material);
                if (index < 0)
                {
                    if (material == WindowMaterial.Default)
                        return _base.CreateDefaultBrush;
                    else
                        return (context) => _base.CreateBrushByMaterial(context, material);
                }
                return _overrideBrushFactoryList[index];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Builder WithVariant(WindowMaterial material, D2D1ColorF brushColor)
            => WithVariant(material, context => context.CreateSolidColorBrush(brushColor));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Builder WithVariant(WindowMaterial material, IThemedBrushFactory brushFactory)
            => WithVariant(material, context => brushFactory.CreateBrushByMaterial(context, material));

        public Builder WithVariant(WindowMaterial material, Func<D2D1DeviceContext, D2D1Brush> brushFactory)
        {
            if (material < WindowMaterial.None || material >= WindowMaterial._Last)
                ArgumentOutOfRangeException.Throw(nameof(material));
            UnwrappableList<byte> variantKeyList = _overrideKeyList;
            UnwrappableList<Func<D2D1DeviceContext, D2D1Brush>> variantBrushFactoryList = _overrideBrushFactoryList;
            byte key = (byte)material;
            int index = variantKeyList.IndexOf(key);
            if (index >= 0)
            {
                variantBrushFactoryList[index] = brushFactory;
                return this;
            }
            variantKeyList.Add(key);
            variantBrushFactoryList.Add(brushFactory);
            return this;
        }

        public Builder Clone() => new Builder(this);

        public IThemedBrushFactory Build()
        {
            UnwrappableList<byte> keyList = _overrideKeyList;
            UnwrappableList<Func<D2D1DeviceContext, D2D1Brush>> brushFactoryList = _overrideBrushFactoryList;
            if (keyList.Count > 0)
                return new NormalImpl(_base, keyList.ToArray(), brushFactoryList.ToArray());
            return _base;
        }

        object ICloneable.Clone() => Clone();
    }
}
