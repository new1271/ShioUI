using System;
using System.Threading;
using System.Threading.Tasks;

namespace ShioUI;

partial class WindowMessageLoop
{
    private sealed class FuncInvokeClosure<TResult> : InvokeClosureBase<Func<TResult>, TResult>
    {
        public FuncInvokeClosure(Func<TResult> @delegate, TaskCompletionSource<TResult>? completionSource, CancellationToken cancellationToken)
            : base(@delegate, completionSource, cancellationToken) { }

        protected override TResult InvokeCore(Func<TResult> invoker) => invoker.Invoke();
    }

    private sealed class FuncInvokeClosure<TArg, TResult> : InvokeClosureBase<Func<TArg, TResult>, TResult>
    {
        private readonly TArg _arg;

        public FuncInvokeClosure(Func<TArg, TResult> @delegate, TArg arg,
            TaskCompletionSource<TResult>? completionSource, CancellationToken cancellationToken)
            : base(@delegate, completionSource, cancellationToken)
        {
            _arg = arg;
        }

        protected override TResult InvokeCore(Func<TArg, TResult> invoker) => invoker.Invoke(_arg);
    }

    private sealed class FuncInvokeClosure<TArg1, TArg2, TResult> : InvokeClosureBase<Func<TArg1, TArg2, TResult>, TResult>
    {
        private readonly TArg1 _arg1;
        private readonly TArg2 _arg2;

        public FuncInvokeClosure(Func<TArg1, TArg2, TResult> @delegate, TArg1 arg1, TArg2 arg2,
            TaskCompletionSource<TResult>? completionSource, CancellationToken cancellationToken)
            : base(@delegate, completionSource, cancellationToken)
        {
            _arg1 = arg1;
            _arg2 = arg2;
        }

        protected override TResult InvokeCore(Func<TArg1, TArg2, TResult> invoker) => invoker.Invoke(_arg1, _arg2);
    }

    private sealed class FuncInvokeClosure<TArg1, TArg2, TArg3, TResult> : InvokeClosureBase<Func<TArg1, TArg2, TArg3, TResult>, TResult>
    {
        private readonly TArg1 _arg1;
        private readonly TArg2 _arg2;
        private readonly TArg3 _arg3;

        public FuncInvokeClosure(Func<TArg1, TArg2, TArg3, TResult> @delegate, TArg1 arg1, TArg2 arg2, TArg3 arg3,
            TaskCompletionSource<TResult>? completionSource, CancellationToken cancellationToken)
            : base(@delegate, completionSource, cancellationToken)
        {
            _arg1 = arg1;
            _arg2 = arg2;
            _arg3 = arg3;
        }

        protected override TResult InvokeCore(Func<TArg1, TArg2, TArg3, TResult> invoker) => invoker.Invoke(_arg1, _arg2, _arg3);
    }
}
