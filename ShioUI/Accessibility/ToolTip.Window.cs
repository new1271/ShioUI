using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

using RiceTea.Core.Extensions;
using RiceTea.Core.Helpers;
using RiceTea.Core.Structures;

using ShioUI.Graphics;
using ShioUI.Graphics.Native.Direct2D;
using ShioUI.Graphics.Native.Direct2D.Brushes;
using ShioUI.Graphics.Native.DirectWrite;
using ShioUI.Internals;
using ShioUI.Internals.Native;
using ShioUI.Internals.NativeHelpers;
using ShioUI.Theme;
using ShioUI.Utils;
using ShioUI.Windows;

namespace ShioUI.Accessibility;

partial class ToolTip
{
    private sealed partial class Window : CoreWindow
    {
        private static readonly string[] _brushNames = new string[(int)Brush._Last]
        {
            "back",
            "fore"
        };

        private readonly D2D1Brush[] _brushes = new D2D1Brush[(nuint)Brush._Last];
        private readonly ToolTip _owner;
        private readonly string _text;
        private readonly Point _locationOnScreen;
        private readonly uint _animationTicks;

        private DWriteTextLayout? _textLayout;

        public Window(ToolTip owner, string text, uint animationTicks, Point locationOnScreen) :
            base(owner._owner, passParentToUnderlyingWindow: true)
        {
            _owner = owner;
            _text = text;
            _animationTicks = animationTicks;
            _locationOnScreen = locationOnScreen;
            WindowMaterial = WindowMaterial.None;
        }

        protected override CreateWindowInfo GetCreateWindowInfo()
        {
            CreateWindowInfo result = base.GetCreateWindowInfo();
            result.Styles &= ~WindowStyles.OverlappedWindow;
            result.Styles |= WindowStyles.Popup | WindowStyles.ClipSiblings;
            result.ExtendedStyles &= ~(WindowExtendedStyles.AppWindow | WindowExtendedStyles.WindowEdge);
            result.ExtendedStyles |= WindowExtendedStyles.TopMost | WindowExtendedStyles.ToolWindow | 
                WindowExtendedStyles.NoActivate;
            return result;
        }

        protected override IEnumerable<UIElement?> EnumerateActiveElements() => Array.Empty<UIElement?>();

        protected override void InitializeElements() { }

        protected override void ApplyThemeCore(IThemeResourceProvider provider)
        {
            ToolTip owner = _owner;

            base.ApplyThemeCore(provider);
            UIElementHelper.ApplyThemeBrushesUnsafe(provider, _brushes, _brushNames, owner._themePrefix, (nuint)Brush._Last);
            DWriteTextLayout layout = CreateTextLayout(provider.FontName);

            if (Screen.TryGetScreenInfoFromHwnd(Handle, out ScreenInfo screenInfo))
            {
                Rect workingArea = screenInfo.WorkingArea;

                Point locationOnScreen = _locationOnScreen;
                Vector2 pointsPerPixel = PointsPerPixel;
                int xOffset = MathI.Round(10 * pointsPerPixel.X, MidpointRounding.AwayFromZero);
                int yOffset = MathI.Round(18 * pointsPerPixel.Y, MidpointRounding.AwayFromZero);
                Size sizeOnScreen = GraphicsUtils.ScalingSizeAndConvert(
                    new SizeF(layout.MaxWidth + UIConstants.ElementMarginDouble, layout.MaxHeight + UIConstants.ElementMarginDouble),
                    pointsPerPixel);

                Rectangle predictedBounds = new Rectangle(locationOnScreen, sizeOnScreen);
                if (predictedBounds.Right >= workingArea.Right)
                    predictedBounds.X -= predictedBounds.Width + xOffset;
                else
                    predictedBounds.X += xOffset;
                if (predictedBounds.Bottom >= workingArea.Bottom)
                    predictedBounds.Y -= predictedBounds.Height + yOffset;
                else
                    predictedBounds.Y += yOffset;

                RawBounds = predictedBounds;
                UpdateAndResize();
            }
        }

        protected override void RecalculateLayout(ref WindowLayoutData data, Size windowSize)
        {
            data.PageBounds = new(Point.Empty, windowSize);
        }

        protected override void RenderTitle(D2D1DeviceContext deviceContext, DirtyAreaCollector collector, bool force, in WindowRenderingData data) { }

        protected override void RenderPageBackground(in RegionalRenderingContext context)
            => GraphicsUtils.ClearAndFill(context, _brushes.AsUnsafeRef()[(nuint)Brush.BackBrush], WindowBaseColor);

        protected override bool IsBackgroundOpaque() => base.IsBackgroundOpaque() || GraphicsUtils.CheckBrushIsSolid(_brushes.AsUnsafeRef()[(nuint)Brush.BackBrush]);

        protected override RenderResult RenderPage(in RegionalRenderingContext context, in WindowRenderingData data)
        {
            context.UsePresentAllModeOnce();

            RenderPageBackground(context, data);

            SizeF size = context.Size;

            DWriteTextLayout? layout = _textLayout;
            if (layout is not null)
            {
                layout.MaxHeight = size.Height;
                layout.MaxWidth = size.Width;
                layout.ParagraphAlignment = DWriteParagraphAlignment.Center;
                layout.TextAlignment = DWriteTextAlignment.Center;

                context.DrawTextLayout(PointF.Empty, layout, _brushes.AsUnsafeRef()[(nuint)Brush.ForeBrush]);
            }
            return RenderResult.Successed;
        }

        protected override void ShowCore(IntPtr handle) => User32.ShowWindow(handle, ShowWindowCommands.ShowNA);

        private DWriteTextLayout CreateTextLayout(string fontName)
        {
            DWriteFactory factory = SharedResources.DWriteFactory;
            using DWriteTextFormat format = factory.CreateTextFormat(fontName, UIConstants.DefaultFontSize);
            DWriteTextLayout layout = GraphicsUtils.CreateCustomTextLayout(_text, format, float.PositiveInfinity);
            DisposeHelper.SwapDisposeAtomic(ref _textLayout, layout);
            return layout;
        }
    }
}
