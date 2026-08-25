using System;
using System.Runtime.CompilerServices;

using RiceTea.Core.Helpers;

using ShioUI.Caching;
using ShioUI.Internals.Native;
using ShioUI.Windows;

namespace ShioUI.Internals;

internal static unsafe class WindowMessageFilterHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryFilterMessage(PumpingMessage* pMsg, CacheStore<IWindowMessageFilter> store, out nint result)
    {
        using CacheStore<IWindowMessageFilter>.Scope scope = store.GetLastSnapshot();
        int count = scope.Count;
        if (count <= 0)
        {
            result = 0;
            return false;
        }
        return Core(&pMsg->body, in scope.GetReferenceOfFirstElement(), count, out result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryFilterMessage(IntPtr hwnd, WindowMessage message, nint wParam, nint lParam, CacheStore<IWindowMessageFilter> store, out nint result)
    {
        using CacheStore<IWindowMessageFilter>.Scope scope = store.GetLastSnapshot();
        int count = scope.Count;
        if (count <= 0)
        {
            result = 0;
            return false;
        }
        PumpingMessageBody body = new PumpingMessageBody()
        {
            hwnd = hwnd,
            message = message,
            wParam = wParam,
            lParam = lParam
        };
        return Core(&body, in scope.GetReferenceOfFirstElement(), count, out result);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool Core(PumpingMessageBody* pMsg, ref readonly IWindowMessageFilter filterRef, int count, out nint result)
    {
        int i = 0;
        do
        {
            IWindowMessageFilter filter = UnsafeHelper.AddTypedOffsetAsReadOnly(in filterRef, i);
            try
            {
                if (filter.TryProcessWindowMessage(pMsg->hwnd, pMsg->message, pMsg->wParam, pMsg->lParam, out result))
                    return true;
            }
            catch (Exception ex)
            {
                MessageLoopExceptionEventHandler? eventHandler = WindowMessageLoop.GetExceptionEventHandler();
                if (eventHandler is null)
                    throw;
                eventHandler.Invoke(filter, new MessageLoopExceptionEventArgs(ex));
            }
        } while (++i < count);

        result = 0;
        return false;
    }
}
