using System.Runtime.InteropServices;
using System.Threading;

using RiceTea.Core;
using RiceTea.Core.Collections;
using RiceTea.Core.Helpers;

using ShioUI.Caching;
using ShioUI.Graphics;
using ShioUI.Utils;

namespace ShioUI.Windows;

public abstract partial class CoreWindow : NativeWindow
{
    protected unsafe CoreWindow() : this(deviceProvider: null) { }

    protected unsafe CoreWindow(GraphicsDeviceProvider? deviceProvider) : base(null)
    {
        _parent = null;
        _activeElementsCacheStore = new(this, &CreateSnapshotForActiveElements, &DropSnapshot);
        _elementsCacheStore = new(this, &CreateSnapshotForElements, &DropSnapshot);
        _windowMessageFilterStore = new(this, &CreateSnapshotForWindowMessageFilter, &DropSnapshot);

        _graphicsDeviceProvider = deviceProvider;
        _windowMaterial = ShioSettings.WindowMaterial;
        UnwrappableList<GCHandle> windowList = _rootWindowList;
        lock (windowList)
            windowList.Add(GCHandle.Alloc(this, GCHandleType.Weak));
        InitUnmanagedPart();
    }

    protected unsafe CoreWindow(CoreWindow? parent, bool passParentToUnderlyingWindow = false) : base(passParentToUnderlyingWindow ? parent : null)
    {
        _parent = parent;
        _activeElementsCacheStore = new(this, &CreateSnapshotForActiveElements, &DropSnapshot);
        _elementsCacheStore = new(this, &CreateSnapshotForElements, &DropSnapshot);
        _windowMessageFilterStore = new(this, &CreateSnapshotForWindowMessageFilter, &DropSnapshot);

        UnwrappableList<GCHandle> windowList;
        if (parent is null)
        {
            _graphicsDeviceProvider = null;
            _windowMaterial = ShioSettings.WindowMaterial;
            windowList = _rootWindowList;
        }
        else
        {
            _graphicsDeviceProvider = parent.GetGraphicsDeviceProvider();
            _windowMaterial = parent.WindowMaterial;
            windowList = parent._childrenReferenceList;
        }
        lock (windowList)
            windowList.Add(GCHandle.Alloc(this, GCHandleType.Weak));
        InitUnmanagedPart();
    }

    protected virtual void DisposeAllElements()
    {
        using CacheStore<UIElement?>.Scope scope = EnterElementsCacheScope();
        UIElementHelper.DisposeForElementsUnsafe(in scope.GetReferenceOfFirstElement(), scope.Count);
    }

    private static void SafeDispose(GCHandle[] handleArray)
    {
        int length = handleArray.Length;
        if (length <= 0)
            return;
        ref GCHandle handleRef = ref UnsafeHelper.GetArrayDataReference(handleArray);
        int i = 0;
        do
        {
            SafeDispose(ref UnsafeHelper.AddTypedOffset(ref handleRef, i));
        } while (++i < length);
    }

    private static void SafeDispose(ref GCHandle handle)
    {
        if (handle.IsAllocated)
            handle.Free();
    }

    protected override void DisposeCore(bool disposing)
    {
        if (disposing)
        {
            DisposeHelper.SwapDisposeAtomicWeak(ref _resourceProvider);
            DisposeHelper.SwapDisposeAtomic(ref _controller);
            DisposeHelper.SwapDisposeAtomic(ref _host);
            DisposeHelper.SwapDisposeAtomic(ref _titleLayout);
            DisposeHelper.DisposeAllUnsafe(in UnsafeHelper.GetArrayDataReference(_brushes), (nuint)Brush._Last);
            GetOverlayElement()?.Dispose();
            DisposeAllElements();

            if (Atomics.Read(ref _recreateGraphicsDeviceProviderBarrier) != 0)
                SpinWait.SpinUntil(() => Atomics.Read(ref _recreateGraphicsDeviceProviderBarrier) != 0);
            if (Atomics.Read(ref _ownedGDP) != 0)
                DisposeHelper.SwapDisposeAtomic(ref _graphicsDeviceProvider);
            else
                Atomics.Write(ref _graphicsDeviceProvider, null);

            _activeElementsCacheStore.Dispose();
            _elementsCacheStore.Dispose();
        }
        _overlayElement = null;

        SafeDispose(_recordedMouseDownHitElementRefs);
        SafeDispose(ref _recordedLastMouseMoveHitElementRef);
        SafeDispose(ref _lastMouseMoveHitElementRef);
        SafeDispose(ref _focusElementRef);
        SequenceHelper.Clear(_brushes);
        base.DisposeCore(disposing);
    }
}
