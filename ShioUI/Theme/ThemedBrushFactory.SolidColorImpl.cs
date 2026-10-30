using System.Collections.Generic;

using ShioUI.Graphics.Native.Direct2D;
using ShioUI.Graphics.Native.Direct2D.Brushes;

namespace ShioUI.Theme;

partial class ThemedBrushFactory
{
    private sealed class SolidColorImpl : IThemedBrushFactory, IThemedColorFactory
    {
        private readonly D2D1ColorF _color;

        public SolidColorImpl(in D2D1ColorF color) => _color = color;

        public D2D1Brush CreateBrushByMaterial(D2D1DeviceContext context, WindowMaterial material) => context.CreateSolidColorBrush(_color);

        public D2D1ColorF CreateColorByMaterial(WindowMaterial material) => _color;

        public D2D1Brush CreateDefaultBrush(D2D1DeviceContext context) => context.CreateSolidColorBrush(_color);

        public D2D1ColorF CreateDefaultColor() => _color;

        public IEnumerable<WindowMaterial> GetVariants() => [WindowMaterial.Default];
    }
}
