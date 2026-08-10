using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Threading;

using RiceTea.Core;

using ShioUI.Layout;
using ShioUI.Theme;
using ShioUI.Utils;
using ShioUI.Windows;

namespace ShioUI;

partial class UIElement
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

    public IRenderWindow Window
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            IElementContainer parent = Parent;
            return parent is IRenderWindow window ? window : parent.Window;
        }
    }

    public CoreWindow RootWindow
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Parent.RootWindow;
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
        get => CheckIsRenderedOnce(Atomics.Read(ref _requestRedraw));
    }

    public IElementContainer Parent
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Atomics.Read(ref _parent);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            if (ReferenceEquals(Atomics.Exchange(ref _parent, value), value))
                return;
            InvalidateLayout();
            ResetRenderCheckFramestamp();
            Update();
        }
    }

    public Point Location
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => BoundsHelper.FastGetLocation(in _bounds);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            using Lock.Scope scope = EnterSyncScope();

            Rectangle bounds = _bounds.GetValueUnsafe();
            if (bounds.Location == value)
                return;

            _bounds.Value = new(location: value, size: bounds.Size);
            OnLocationChanged();
            AfterBoundsChanged();
        }
    }

    public Size Size
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => BoundsHelper.FastGetSize(in _bounds);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            using Lock.Scope scope = EnterSyncScope();

            Rectangle bounds = _bounds.GetValueUnsafe();
            if (bounds.Size == value)
                return;

            _bounds.Value = new(location: bounds.Location, size: value);
            OnSizeChanged();
            AfterBoundsChanged();
        }
    }

    public Rectangle Bounds
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _bounds.Value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            using Lock.Scope scope = EnterSyncScope();

            SetBoundsCore(value);
            AfterBoundsChanged();
        }
    }

    public int X
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => BoundsHelper.FastGetX(in _bounds);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            using Lock.Scope scope = EnterSyncScope();

            Rectangle bounds = _bounds.GetValueUnsafe();
            if (bounds.X == value)
                return;

            _bounds.Value = new(location: new(x: value, bounds.Y), size: bounds.Size);
            OnLocationChanged();
            AfterBoundsChanged();
        }
    }

    public int Left
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => X;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => X = value;
    }

    public LayoutNode LeftDefinition
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetLayoutDefinitionCore((nuint)LayoutProperty.Left);
    }

    public LayoutNode? LeftExpression
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetLayoutExpressionCore((nuint)LayoutProperty.Left);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => SetLayoutExpressionCore((nuint)LayoutProperty.Left, value);
    }

    public int Y
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => BoundsHelper.FastGetY(in _bounds);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            using Lock.Scope scope = EnterSyncScope();

            Rectangle bounds = _bounds.GetValueUnsafe();
            if (bounds.Y == value)
                return;

            _bounds.Value = new(location: new(bounds.X, y: value), size: bounds.Size);
            OnLocationChanged();
            AfterBoundsChanged();
        }
    }

    public int Top
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Y;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => Y = value;
    }

    public LayoutNode TopDefinition
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetLayoutDefinitionCore((nuint)LayoutProperty.Top);
    }

    public LayoutNode? TopExpression
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetLayoutExpressionCore((nuint)LayoutProperty.Top);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => SetLayoutExpressionCore((nuint)LayoutProperty.Top, value);
    }

    public int Right
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _bounds.Value.Right;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            using Lock.Scope scope = EnterSyncScope();

            Rectangle bounds = _bounds.GetValueUnsafe();
            int left = bounds.Left;
            int width = bounds.Width;
            int newWidth = value - left;

            if (width == newWidth)
                return;
            if (newWidth < 0)
                ArgumentOutOfRangeException.Throw(nameof(value));

            _bounds.Value = new(location: bounds.Location, size: new(width: newWidth, height: bounds.Height));
            OnSizeChanged();
            AfterBoundsChanged();
        }
    }

    public LayoutNode RightDefinition
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetLayoutDefinitionCore((nuint)LayoutProperty.Right);
    }

    public LayoutNode? RightExpression
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetLayoutExpressionCore((nuint)LayoutProperty.Right);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => SetLayoutExpressionCore((nuint)LayoutProperty.Right, value);
    }

    public int Bottom
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _bounds.Value.Bottom;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            using Lock.Scope scope = EnterSyncScope();

            Rectangle bounds = _bounds.GetValueUnsafe();
            int top = bounds.Top;
            int height = bounds.Height;
            int newHeight = value - top;

            if (height == newHeight)
                return;
            if (newHeight < 0)
                ArgumentOutOfRangeException.Throw(nameof(value));

            _bounds.Value = new(location: bounds.Location, size: new(width: bounds.Width, height: newHeight));
            OnSizeChanged();
            AfterBoundsChanged();
        }
    }

    public LayoutNode BottomDefinition
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetLayoutDefinitionCore((nuint)LayoutProperty.Bottom);
    }

    public LayoutNode? BottomExpression
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetLayoutExpressionCore((nuint)LayoutProperty.Bottom);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => SetLayoutExpressionCore((nuint)LayoutProperty.Bottom, value);
    }

    public int Width
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => BoundsHelper.FastGetWidth(in _bounds);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            using Lock.Scope scope = EnterSyncScope();

            Rectangle bounds = _bounds.GetValueUnsafe();
            if (bounds.Width == value)
                return;

            _bounds.Value = new(location: bounds.Location, size: new(width: value, height: bounds.Height));
            OnSizeChanged();
            AfterBoundsChanged();
        }
    }

    public LayoutNode WidthDefinition
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetLayoutDefinitionCore((nuint)LayoutProperty.Width);
    }

    public LayoutNode? WidthExpression
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetLayoutExpressionCore((nuint)LayoutProperty.Width);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => SetLayoutExpressionCore((nuint)LayoutProperty.Width, value);
    }

    public int Height
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => BoundsHelper.FastGetHeight(in _bounds);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            using Lock.Scope scope = EnterSyncScope();

            Rectangle bounds = _bounds.GetValueUnsafe();
            if (bounds.Height == value)
                return;

            _bounds.Value = new(location: bounds.Location, size: new(width: bounds.Width, height: value));
            OnSizeChanged();
            AfterBoundsChanged();
        }
    }

    public LayoutNode HeightDefinition
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetLayoutDefinitionCore((nuint)LayoutProperty.Height);
    }

    public LayoutNode? HeightExpression
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetLayoutExpressionCore((nuint)LayoutProperty.Height);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => SetLayoutExpressionCore((nuint)LayoutProperty.Height, value);
    }

    public IThemeContext? CurrentTheme
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Atomics.Read(ref _themeContext);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            if (ReferenceEquals(Atomics.Exchange(ref _themeContext, value), value))
                return;
            lock (_themeAccessLock)
            {
                if (!ReferenceEquals(Atomics.Read(ref _themeContext), value))
                    return;
                ApplyThemeContext(value);
            }
        }
    }

    public string ThemePrefix
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _themePrefix;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        init => _themePrefix = value;
    }

    public object? Tag
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Atomics.Read(ref _tag);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => Atomics.Write(ref _tag, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetProperty(LayoutProperty property)
        => property switch
        {
            LayoutProperty.Left => Left,
            LayoutProperty.Top => Top,
            LayoutProperty.Right => Right,
            LayoutProperty.Bottom => Bottom,
            LayoutProperty.Height => Height,
            LayoutProperty.Width => Width,
            _ => ArgumentOutOfRangeException.Throw<int>(nameof(property)),
        };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetProperty(LayoutProperty property, int value)
    {
        switch (property)
        {
            case LayoutProperty.Left:
                Left = value;
                break;
            case LayoutProperty.Top:
                Top = value;
                break;
            case LayoutProperty.Right:
                Right = value;
                break;
            case LayoutProperty.Bottom:
                Bottom = value;
                break;
            case LayoutProperty.Height:
                Height = value;
                break;
            case LayoutProperty.Width:
                Width = value;
                break;
            default:
                ArgumentOutOfRangeException.Throw(nameof(property));
                break;
        }
    }
}
