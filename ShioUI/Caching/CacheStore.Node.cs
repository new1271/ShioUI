using System.Threading;

using RiceTea.Core;
using RiceTea.Core.Helpers;

namespace ShioUI.Caching;

partial class CacheStore<T>
{
    private sealed class Node
    {
        public CacheStore<T>? Owner;
        public Body Body;
        public ulong Timestamp;

        private nuint _refCount;
        private nuint _barrier;

        public T[]? Array => Body.Array;
        public int Count => Body.Count;

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
            Body = default;
            Timestamp = 0;
        }
    }
}
