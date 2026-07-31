using System.Threading;

using RiceTea.Core;
using RiceTea.Core.Helpers;

namespace ShioUI.Internals;

partial class CacheStore<T>
{
    public sealed class CacheNode
    {
        public CacheStore<T>? Owner;
        public T[]? Array;
        public int Count;
        public ulong Timestamp;

        private nuint _refCount;
        private nuint _barrier;

        public void AddRef() => _refCount = MathHelper.Min(_refCount + 1, UnsafeHelper.GetMaxValue<nuint>());

        public bool RemoveRef()
        {
            nuint refCount = _refCount;
            _refCount = MathHelper.Max(refCount - 1, 0);
            return refCount != 1;
        }

        public void EnterBarrier()
        {
            ref nuint barrier = ref _barrier;
            while (Atomics.Exchange(ref barrier, 1) != 0)
            {
                SpinWait waiter = new SpinWait();
                do
                {
                    waiter.SpinOnce();
                } while (Atomics.Read(ref barrier) != 0);
            }
        }

        public void ExitBarrier() => Atomics.Exchange(ref _barrier, 0);

        public void CleanUp()
        {
            Owner = null;
            Array = null;
            Count = 0;
            Timestamp = 0;
        }
    }
}
