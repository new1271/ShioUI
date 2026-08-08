using System;
using System.Collections.Generic;
using System.Drawing;

using RiceTea.Core.Buffers;

using ShioUI.Graphics.Native.Direct2D;

namespace ShioUI.Theme;

partial class DefaultThemeProvider
{
    private sealed class DarkThemeContext : ThemeContextBase
    {
        public override bool IsDarkTheme => true;

        public DarkThemeContext() { }

        private DarkThemeContext(DarkThemeContext original) : base(original) { }

        public override IThemeContext Clone() => new DarkThemeContext(this);

        protected override void OnThemeBuilding(
            PooledList<ThemedColorsBuildingHandler> colorsBuildingHandlerList,
            PooledList<ThemedBrushesBuildingHandler> brushesBuildingHandlerList)
            => ShioSettings.OnDarkThemeBuilding(colorsBuildingHandlerList, brushesBuildingHandlerList);

        public override IEnumerable<KeyValuePair<string, IThemedColorFactory>> BuildColorFactories(Func<string, IThemedColorFactory> queryFunc)
        {
            yield return new KeyValuePair<string, IThemedColorFactory>(
                key: ThemeConstants.WindowBaseColorNode,
                value: ThemedColorFactory.FromColor(new D2D1ColorF(40, 40, 40)));
            yield return new KeyValuePair<string, IThemedColorFactory>(
                key: ThemeConstants.ClearDCColorNode,
                value: ThemedColorFactory.CreateBuilder(new D2D1ColorF(64, 64, 64))
                    .WithVariant(WindowMaterial.MicaAlt, Color.Transparent)
                    .WithVariant(WindowMaterial.Mica, Color.Transparent)
                    .WithVariant(WindowMaterial.Acrylic, new D2D1ColorF(64, 64, 64, 72))
                    .WithVariant(WindowMaterial.Gaussian, new D2D1ColorF(64, 64, 64, 145))
                    .WithVariant(WindowMaterial.Integrated, new D2D1ColorF(0, 0, 0, 0))
                    .Build());
            yield return new KeyValuePair<string, IThemedColorFactory>(
                key: ThemeConstants.WizardWindowBaseColor,
                value: ThemedColorFactory.CreateBuilder(new D2D1ColorF(64, 64, 64))
                    .WithVariant(WindowMaterial.MicaAlt, Color.Transparent)
                    .WithVariant(WindowMaterial.Mica, Color.Transparent)
                    .WithVariant(WindowMaterial.Acrylic, new D2D1ColorF(64, 64, 64, 72))
                    .WithVariant(WindowMaterial.Gaussian, new D2D1ColorF(64, 64, 64, 145))
                    .WithVariant(WindowMaterial.Integrated, new D2D1ColorF(40, 40, 40, 96))
                    .Build());
        }

        public override IEnumerable<KeyValuePair<string, IThemedBrushFactory>> BuildBrushFactories(
            Func<string, IThemedColorFactory> queryColorFunc, Func<string, IThemedBrushFactory> queryBrushFunc)
        {
            // 視窗基礎筆刷
            yield return new KeyValuePair<string, IThemedBrushFactory>(
                key: "app.title.back",
                value: ThemedBrushFactory.CreateBuilder(Color.Black)
                    .WithVariant(WindowMaterial.MicaAlt, Color.Transparent)
                    .WithVariant(WindowMaterial.Mica, Color.Transparent)
                    .WithVariant(WindowMaterial.Acrylic, new D2D1ColorF(20, 20, 20, 32))
                    .WithVariant(WindowMaterial.Gaussian, new D2D1ColorF(20, 20, 20, 64))
                    .WithVariant(WindowMaterial.Integrated, new D2D1ColorF(0, 0, 0, 0))
                    .Build());
            yield return new KeyValuePair<string, IThemedBrushFactory>(
                key: "app.menu.back",
                value: ThemedBrushFactory.CreateBuilder(queryBrushFunc("app.title.back"))
                    .WithVariant(WindowMaterial.Integrated, new D2D1ColorF(20, 20, 20, 100))
                    .Build());
            yield return new KeyValuePair<string, IThemedBrushFactory>(
                key: "app.title.fore.active",
                value: ThemedBrushFactory.FromColor(new D2D1ColorF(245, 245, 245)));
            yield return new KeyValuePair<string, IThemedBrushFactory>(
                key: "app.title.fore.deactive",
                value: ThemedBrushFactory.FromColor(new D2D1ColorF(175, 175, 175)));
            yield return new KeyValuePair<string, IThemedBrushFactory>(
                key: "app.title.closeButton.active",
                value: ThemedBrushFactory.FromColor(new D2D1ColorF(241, 59, 75)));

            // 通用元件
            yield return new KeyValuePair<string, IThemedBrushFactory>(
                key: "app.control.back",
                value: ThemedBrushFactory.FromColorFactory(queryColorFunc(ThemeConstants.WindowBaseColorNode)));
            yield return new KeyValuePair<string, IThemedBrushFactory>(
                key: "app.control.back.disabled",
                value: ThemedBrushFactory.AmplifiedFrom(queryBrushFunc("app.control.back"), 0.7f));
            yield return new KeyValuePair<string, IThemedBrushFactory>(
                key: "app.control.back.hovered",
                value: ThemedBrushFactory.AmplifiedFrom(queryBrushFunc("app.control.back"), 1.1f));
            yield return new KeyValuePair<string, IThemedBrushFactory>(
                key: "app.control.border",
                value: ThemedBrushFactory.FromColor(new D2D1ColorF(165, 165, 165)));
            yield return new KeyValuePair<string, IThemedBrushFactory>(
                key: "app.control.border.active",
                value: ThemedBrushFactory.FromColor(new D2D1ColorF(96, 162, 252)));
            yield return new KeyValuePair<string, IThemedBrushFactory>(
                key: "app.control.fore",
                value: ThemedBrushFactory.FromColor(Color.White));
            yield return new KeyValuePair<string, IThemedBrushFactory>(
                key: "app.control.fore.inactive",
                value: ThemedBrushFactory.FromColor(new D2D1ColorF(203, 203, 203)));
            yield return new KeyValuePair<string, IThemedBrushFactory>(
                key: "app.control.fore.description",
                value: ThemedBrushFactory.FromColor(Color.LightGray));

            // 目錄
            yield return new KeyValuePair<string, IThemedBrushFactory>(
                key: "app.menu.fore",
                value: ThemedBrushFactory.FromColor(new D2D1ColorF(235, 235, 235)));
            yield return new KeyValuePair<string, IThemedBrushFactory>(
                key: "app.menu.fore.active",
                value: ThemedBrushFactory.FromColor(new D2D1ColorF(235, 235, 235)));
            yield return new KeyValuePair<string, IThemedBrushFactory>(
                key: "app.menu.itemSelected.back",
                value: ThemedBrushFactory.FromColor(new D2D1ColorF(120, 120, 120, 200)));
            yield return new KeyValuePair<string, IThemedBrushFactory>(
                key: "app.menu.itemHovered.back",
                value: ThemedBrushFactory.FromColor(new D2D1ColorF(120, 120, 120, 145)));
        }
    }
}
