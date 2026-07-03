using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using RiceTea.Core.Buffers;
using RiceTea.Core.Helpers;
using RiceTea.Core.Native;

namespace ShioUI.Layout;

partial struct VirtualLayoutContext
{
    [StructLayout(LayoutKind.Auto)]
    internal unsafe ref struct SharedData
    {
        public LayoutNode[]? FakeLayoutNodeKeys;
        public FractionalLayoutNode[]? FakeFractionalLayoutNodeKeys;
        public int* FakeLayoutNodeValues;
        public float* FakeFractionalLayoutNodeValues;
        public int FakeLayoutNodeCount, FakeFractionalLayoutNodeCount;
    }

    [StructLayout(LayoutKind.Auto)]
    internal unsafe ref struct Data : IDisposable
    {
        public ArrayPool<LayoutNode>? NodePool;
        public ArrayPool<FractionalLayoutNode>? FractionalNodePool;
        public NativeMemoryPool? MemoryPool;
        public SharedData SharedData;
        public nuint FakeLayoutNodeValuesLength, FakeFractionalLayoutNodeValuesLength;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            ref SharedData data = ref SharedData;

            ArrayPool<LayoutNode>? nodePool = ReferenceHelper.Exchange(ref NodePool, null);
            if (nodePool is not null)
            {
                LayoutNode[]? keys = ReferenceHelper.Exchange(ref data.FakeLayoutNodeKeys, null);
                DebugHelper.ThrowIf(keys is null);
                nodePool.Return(keys);
            }

            ArrayPool<FractionalLayoutNode>? fractionalNodePool = ReferenceHelper.Exchange(ref FractionalNodePool, null);
            if (fractionalNodePool is not null)
            {
                FractionalLayoutNode[]? keys = ReferenceHelper.Exchange(ref data.FakeFractionalLayoutNodeKeys, null);
                DebugHelper.ThrowIf(keys is null);
                fractionalNodePool.Return(keys);
            }

            NativeMemoryPool? memoryPool = ReferenceHelper.Exchange(ref MemoryPool, null);
            if (memoryPool is not null)
            {
                {
                    int* values = ReferenceHelper.Exchange(ref data.FakeLayoutNodeValues, null);
                    if (values is not null)
                        memoryPool.Return(new NativeMemoryBlock(values, ReferenceHelper.Exchange(ref FakeLayoutNodeValuesLength, default) * sizeof(int)));
                }
                {
                    float* values = ReferenceHelper.Exchange(ref data.FakeFractionalLayoutNodeValues, null);
                    if (values is not null)
                        memoryPool.Return(new NativeMemoryBlock(values, ReferenceHelper.Exchange(ref FakeLayoutNodeValuesLength, default) * sizeof(float)));
                }
            }
        }
    }
}
