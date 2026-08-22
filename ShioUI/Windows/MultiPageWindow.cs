using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

using RiceTea.Core;
using RiceTea.Core.Extensions;

using ShioUI.Graphics;
using ShioUI.Utils;

namespace ShioUI.Windows;

public abstract partial class MultiPageWindow : CoreWindow
{
    #region Fields
    private readonly Lock _pageLock = new Lock();
    private nuint _alreadyShown;
    private uint _pageIndex;
    #endregion

    #region Properties
    public abstract uint PageCount { get; }

    public uint CurrentPage
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Atomics.Read(ref _pageIndex);
        set
        {
            uint oldIndex = Atomics.Exchange(ref _pageIndex, value);
            if (oldIndex == value)
                return;
            lock (_pageLock)
            {
                using BatchUpdateScope scope = EnterBatchUpdateScope();

                ClearFocusElement();
                OnCurrentPageChanged(new CurrentPageChangedEventArgs(oldIndex, value));
            }
        }
    }
    #endregion

    #region Events
    #endregion

    #region Constuctor       
    protected MultiPageWindow() : base() { }

    protected MultiPageWindow(GraphicsDeviceProvider? deviceProvider) : base(deviceProvider) { }

    protected MultiPageWindow(CoreWindow? parent, bool passParentToUnderlyingWindow = false) : base(parent, passParentToUnderlyingWindow) { }
    #endregion

    #region Override Methods
    protected override IEnumerable<UIElement?> EnumerateActiveElements()
    {
        uint pageIndex = _pageIndex;
        return EnumerateActiveElements(pageIndex);
    }

    protected override IEnumerable<UIElement?> EnumerateElements()
    {
        uint pageCount = PageCount;
        if (pageCount <= 0)
            return Enumerable.Empty<UIElement?>();
        IEnumerable<UIElement?> elements = EnumerateActiveElements(0);
        for (uint i = 1; i < pageCount; i++)
            elements = elements.ConcatOptimized(EnumerateActiveElements(i));
        return elements;
    }

    protected override void RecalculatePageLayout(Size pageSize, in RecalculateLayoutInformation information)
        => RecalculatePageLayout(pageSize, _pageIndex, information);

    protected override void OnShown()
    {
        base.OnShown();

        if (WindowMessageLoop.IsMessageLoopThread) // ShioUI 觸發的 OnShown 必定在視窗訊息執行緒
            WindowMessageLoop.InvokeAsync(static _this => _this.OnShown_RunLater(), this); // 脫離目前上下文後再執行，避免被使用者程式碼影響
    }
    #endregion

    #region Virtual Methods
    protected virtual void RecalculatePageLayout(Size pageSize, uint pageIndex, in RecalculateLayoutInformation information)
        => base.RecalculatePageLayout(pageSize, information);
    #endregion

    #region Abstract Methods
    protected abstract IEnumerable<UIElement?> EnumerateActiveElements(uint pageIndex);
    #endregion

    #region Normal Methods
    private void OnShown_RunLater()
    {
        lock (_pageLock)
        {
            Volatile.Write(ref _alreadyShown, Booleans.TrueNativeUnsigned);
            uint pageIndex = Volatile.Read(ref _pageIndex);

            using BatchUpdateScope scope = EnterBatchUpdateScope();

            ClearFocusElement();
            OnCurrentPageChanged(new CurrentPageChangedEventArgs(pageIndex, pageIndex));
        }
    }
    #endregion
}
