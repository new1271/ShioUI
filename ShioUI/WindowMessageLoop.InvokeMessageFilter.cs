using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

using ShioUI.Internals;
using ShioUI.Windows;

using RiceTea.Core;
using RiceTea.Core.Helpers;
using RiceTea.Core.Threading;

#if NET472_OR_GREATER
using RiceTea.Core.Extensions;
#endif

namespace ShioUI;

partial class WindowMessageLoop
{
    private sealed class InvokeMessageFilter : IWindowMessageFilter
    {
        public static readonly InvokeMessageFilter Instance = new InvokeMessageFilter();

        private readonly Swapable<Queue<IInvokeClosure>> _invokeClosureQueue = Swapable.CreateQueue<IInvokeClosure>(optimistic: true);

        private int _readBarrier;

        private InvokeMessageFilter() => _readBarrier = 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddInvoke(IInvokeClosure closure)
        {
            Queue<IInvokeClosure> queue = _invokeClosureQueue.Value;
            lock (queue)
                queue.Enqueue(closure);
        }

        public bool TryProcessWindowMessage(IntPtr hwnd, WindowMessage message, nint wParam, nint lParam, out nint result)
        {
            result = 0;
            if (hwnd != IntPtr.Zero || (uint)message != CustomWindowMessages.ShioUI_WindowInvoke)
                return false;

            ProcessAllInvoke();
            return true;
        }

        public void ProcessAllInvoke()
        {
            if (InterlockedHelper.CompareExchange(ref _readBarrier, Booleans.TrueInt, Booleans.FalseInt) != Booleans.FalseInt)
                return;

            Queue<IInvokeClosure> queue = _invokeClosureQueue.Swap();
            try
            {
                lock (queue)
                {
                    while (queue.TryDequeue(out IInvokeClosure? closure))
                    {
                        if (closure is not null)
                            DoInvoke(closure);
                    }
                }
            }
            finally
            {
                Interlocked.Exchange(ref _readBarrier, Booleans.FalseInt);
            }
        }

        private void DoInvoke(IInvokeClosure closure)
        {
            try
            {
                closure.Invoke();
            }
            catch (Exception ex)
            {
                MessageLoopExceptionEventHandler? eventHandler = ExceptionCaught;
                if (eventHandler is null)
                    throw;
                eventHandler.Invoke(this, new MessageLoopExceptionEventArgs(ex));
            }
        }
    }
}
