using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

using InlineMethod;

using RiceTea.Core;
using RiceTea.Core.Helpers;
using RiceTea.Core.Structures;
using RiceTea.Core.Threading;

using ShioUI.Graphics;
using ShioUI.Graphics.Native.Direct2D.Brushes;
using ShioUI.Layout;
using ShioUI.Layout.Internals;
using ShioUI.Theme;
using ShioUI.Traits;
using ShioUI.Utils;

namespace ShioUI;

public abstract partial class UIElement : RenderElement
{
    private readonly LayoutNode?[] _layoutDefinitions = new LayoutNode?[(int)LayoutProperty._Last];
    private readonly LayoutNode?[] _layoutExpressions = new LayoutNode?[(int)LayoutProperty._Last];
    private readonly Lock _syncLock = new Lock(), _themeAccessLock = new Lock();
    private readonly string _themePrefix;

    private WeakReference<UIElement>? _reference;
    private IElementContainer _parent;
    private IThemeContext? _themeContext;
    private object? _tag;
    private GCHandle _themeResourceProviderReference;
    private StateTiny.SingleWriter<Rectangle> _bounds; // 使用外部鎖來保證單一寫入
    private ulong _layoutFramestamp, _renderCheckFramestamp;

    public UIElement(IElementContainer parent, string themePrefix)
    {
        _parent = parent;
        _themePrefix = themePrefix;
        _themeResourceProviderReference = GCHandle.Alloc(null, GCHandleType.Weak);
    }

    [Inline(InlineBehavior.Remove)]
    private WeakReference<UIElement> GetWeakReference() => _reference ??= new WeakReference<UIElement>(this);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public LayoutNode GetLayoutDefinition(LayoutProperty property)
    {
        if (property >= LayoutProperty._Last)
            return ArgumentOutOfRangeException.Throw<LayoutNode>(nameof(property));
        return GetLayoutDefinitionCore((nuint)property);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public LayoutNode? GetLayoutExpression(LayoutProperty property)
    {
        if (property >= LayoutProperty._Last)
            return ArgumentOutOfRangeException.Throw<LayoutNode>(nameof(property));
        return GetLayoutExpressionCore((nuint)property);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetLayoutExpression(LayoutProperty property, LayoutNode? variable)
    {
        if (property >= LayoutProperty._Last)
        {
            Throw();
            return;
        }
        SetLayoutExpressionCore((nuint)property, variable);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void Throw() => ArgumentOutOfRangeException.Throw(nameof(property));
    }

    [Inline(InlineBehavior.Remove)]
    private LayoutNode GetLayoutDefinitionCore(nuint property)
    {
        ref LayoutNode? variable = ref UnsafeHelper.AddTypedOffset(ref UnsafeHelper.GetArrayDataReference(_layoutDefinitions), property);
        return variable ??= new UIElementLayoutNode(GetWeakReference(), (LayoutProperty)property);
    }

    [Inline(InlineBehavior.Remove)]
    private LayoutNode? GetLayoutExpressionCore(nuint property)
        => Atomics.Read(ref UnsafeHelper.AddTypedOffset(ref UnsafeHelper.GetArrayDataReference(_layoutExpressions), property));

    [Inline(InlineBehavior.Remove)]
    private void SetLayoutExpressionCore(nuint property, LayoutNode? variable)
    {
        Atomics.Write(ref UnsafeHelper.AddTypedOffset(ref UnsafeHelper.GetArrayDataReference(_layoutExpressions), property), variable);
        InvalidateLayout();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void EnsureThemeIsApplied()
    {
        if (Atomics.Read(ref _themeContext) is not null)
            return;
        IThemeResourceProvider? provider = Window.GetDefaultThemeResourceProvider();
        if (provider is null)
            return;
        lock (_themeAccessLock)
        {
            if (ReferenceEquals(_themeResourceProviderReference.Target, provider))
                return;
            _themeResourceProviderReference.Target = provider;
            lock (_syncLock)
                ApplyThemeCore(provider);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void InvalidateLayout() => Atomics.Write(ref _layoutFramestamp, 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RefreshLayout(ulong framestamp) => Atomics.Write(ref _layoutFramestamp, framestamp);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RefreshLayout(in Rectangle bounds, ulong framestamp)
    {
        using Lock.Scope scope = EnterSyncScope();

        SetBoundsCore(bounds);
        RefreshLayout(framestamp);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool CheckLayoutOutdated(ulong framestamp)
    {
        if (Atomics.Read(ref _layoutFramestamp) != framestamp || Atomics.Read(ref _themeContext) is not null)
            return true;

        IThemeResourceProvider? provider = Window.GetDefaultThemeResourceProvider();
        if (provider is null)
            return false;
        lock (_themeAccessLock)
            return !ReferenceEquals(_themeResourceProviderReference.Target, provider);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ResetRenderCheckFramestamp()
        => Atomics.Write(ref _renderCheckFramestamp, 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SyncRenderCheckFramestamp(ulong framestamp)
        => Atomics.Write(ref _renderCheckFramestamp, framestamp);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool TrySyncRenderCheckFramestamp(ulong oldFramestamp, ulong newFramestamp)
        => Atomics.CompareExchange(ref _renderCheckFramestamp, newFramestamp, oldFramestamp) == oldFramestamp;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBackgroundOpaque() => IsBackgroundOpaqueCore() || Parent.IsBackgroundOpaque(this);

    protected virtual bool IsBackgroundOpaqueCore() => false;

    protected override void UpdateCore() => Window.Refresh();

    public void Render(in RegionalRenderingContext context, ulong framestamp)
    {
        try
        {
            Render(context);
        }
        finally
        {
            SyncRenderCheckFramestamp(framestamp);
        }
    }

    protected void RenderBackground(in RegionalRenderingContext context) => Parent.RenderBackground(this, in context);

    protected void RenderBackground(in RegionalRenderingContext context, D2D1Brush backBrush)
    {
        if (backBrush is D2D1SolidColorBrush solidColorBrush)
        {
            if (GraphicsUtils.CheckBrushIsSolid(solidColorBrush))
            {
                context.Clear(solidColorBrush.Color);
                return;
            }
            RenderBackground(context);
            context.FillRectangle(RectF.FromXYWH(PointF.Empty, context.Size), backBrush);
            return;
        }
        bool isSolidBrush = backBrush switch
        {
            D2D1LinearGradientBrush linearGradientBrush => GraphicsUtils.CheckBrushIsSolid(linearGradientBrush),
            D2D1RadialGradientBrush radialGradientBrush => GraphicsUtils.CheckBrushIsSolid(radialGradientBrush),
            _ => false
        };
        if (!isSolidBrush)
            RenderBackground(context);
        context.FillRectangle(RectF.FromXYWH(PointF.Empty, context.Size), backBrush);
    }

    public virtual void OnLocationChanged() { }

    public virtual void OnSizeChanged() { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ApplyTheme(IThemeResourceProvider provider)
    {
        if (Atomics.Read(ref _themeContext) is not null)
            return;

        lock (_themeAccessLock)
        {
            _themeResourceProviderReference.Target = provider;
            lock (_syncLock)
                ApplyThemeCore(provider);
        }
        Update();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ApplyThemeContext(IThemeContext? value)
    {
        IElementContainer parent = Parent;
        IRenderWindow window = parent.Window;

        if (value is null)
        {
            IThemeResourceProvider? provider = window.GetDefaultThemeResourceProvider();
            _themeResourceProviderReference.Target = provider;
            if (provider is not null)
            {
                lock (_syncLock)
                    ApplyThemeCore(provider);
            }
        }
        else
        {
            _themeResourceProviderReference.Target = null;

            IThemeResourceProvider provider = window.CreateThemeResourceProvider(value);
            try
            {
                lock (_syncLock)
                    ApplyThemeCore(provider);
            }
            finally
            {
                (provider as IDisposable)?.Dispose();
            }
        }
        Update();
    }

    protected abstract void ApplyThemeCore(IThemeResourceProvider provider);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetBoundsCore(in Rectangle value)
    {
        Rectangle bounds = _bounds.GetValueUnsafe();
        if (bounds == value)
            return;
        _bounds.Value = value;
        if (bounds.Location != value.Location)
            OnLocationChanged();
        if (bounds.Size != value.Size)
            OnSizeChanged();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AfterBoundsChanged()
    {
        ResetRenderCheckFramestamp();
        Update();
    }

    protected override void DisposeCore(bool disposing)
    {
        lock (_themeAccessLock)
            _themeResourceProviderReference.Free();
    }
}
