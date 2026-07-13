using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

using ShioUI.Internals;
using ShioUI.Windows;

using RiceTea.Core;
using RiceTea.Core.Helpers;
using RiceTea.Core.Threading;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using System.Threading.Tasks;




#if NET472_OR_GREATER
using RiceTea.Core.Extensions;
#endif

namespace ShioUI;

partial class WindowMessageLoop
{
    private sealed class ShowDialogMessageFilter : IWindowMessageFilter
    {
        public static readonly ShowDialogMessageFilter Instance = new ShowDialogMessageFilter();

        private ShowDialogMessageFilter() { }

        public bool TryProcessWindowMessage(IntPtr hwnd, WindowMessage message, nint wParam, nint lParam, out nint result)
        {
            result = 0;
            if (hwnd != IntPtr.Zero || (uint)message != CustomWindowMessages.ShioUI_CallShowDialog)
                return false;

            ConcurrentStack<GCHandle> stack = _handleStackForShowDialogMessage;
            GCHandle handle = GCHandle.FromIntPtr(wParam);
            GCHandle handle2 = GCHandle.FromIntPtr(lParam);
            if (handle.IsAllocated)
            {
                try
                {
                    if (handle2.IsAllocated)
                    {
                        try
                        {
                            if (handle.Target is NativeWindow window && handle2.Target is TaskCompletionSource<bool> completionSource)
                            {
                                try
                                {
                                    NativeWindow.ShowDialogInternal(window);
                                    completionSource.TrySetResult(true);
                                }
                                catch (Exception ex)
                                {
                                    completionSource.TrySetException(ex);
                                }
                            }
                        }
                        finally
                        {
                            handle2.Target = null;
                            stack.Push(handle2);
                        }
                    }
                }
                finally
                {
                    handle.Target = null;
                    stack.Push(handle);
                }
            }
            else if (handle2.IsAllocated)
            {
                handle2.Target = null;
                stack.Push(handle2);
            }

            return true;
        }
    }
}
