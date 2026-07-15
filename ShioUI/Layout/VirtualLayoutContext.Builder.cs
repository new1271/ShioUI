using System;
using System.Runtime.CompilerServices;

using RiceTea.Core.Buffers;
using RiceTea.Core.Extensions;
using RiceTea.Core.Helpers;
using RiceTea.Core.Native;

namespace ShioUI.Layout;

partial struct VirtualLayoutContext
{
    public unsafe ref struct Builder : IDisposable
    {
        internal LayoutContext.Arguments _arguments;
        internal Data _data;

        public Builder(scoped in LayoutContext context)
        {
            _arguments = context._arguments;

            ref readonly SharedData virtualData = ref context._virtualData;
            {
                int count = virtualData.FakeLayoutNodeCount;
                if (count > 0)
                {
                    LayoutNode[]? keys = virtualData.FakeLayoutNodeKeys;
                    int* values = virtualData.FakeLayoutNodeValues;
                    DebugHelper.ThrowIf(keys is null || values is null);
                    ref readonly LayoutNode keysRef = ref UnsafeHelper.GetArrayDataReference(keys);
                    for (int i = 0; i < count; i++)
                        SetFakeNodeValue(UnsafeHelper.AddTypedOffsetAsReadOnly(in keysRef, i), values[i]);
                }
            }
            {
                int count = virtualData.FakeFractionalLayoutNodeCount;
                if (count > 0)
                {
                    FractionalLayoutNode[]? keys = virtualData.FakeFractionalLayoutNodeKeys;
                    float* values = virtualData.FakeFractionalLayoutNodeValues;
                    DebugHelper.ThrowIf(keys is null || values is null);
                    ref readonly FractionalLayoutNode keysRef = ref UnsafeHelper.GetArrayDataReference(keys);
                    for (int i = 0; i < count; i++)
                        SetFakeNodeValue(UnsafeHelper.AddTypedOffsetAsReadOnly(in keysRef, i), values[i]);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public VirtualLayoutContext Build() => new VirtualLayoutContext(ref this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetFakeNodeValue(LayoutNode node, int value)
        {
            int count = _data.SharedData.FakeLayoutNodeCount;
            if (count <= 0)
                SetFakeNodeValueFast(node, value);
            else
                SetFakeNodeValueSlow(node, value, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetFakeNodeValue(FractionalLayoutNode node, float value)
        {
            int count = _data.SharedData.FakeFractionalLayoutNodeCount;
            if (count <= 0)
                SetFakeNodeValueFast(node, value);
            else
                SetFakeNodeValueSlow(node, value, count);
        }

        private void SetFakeNodeValueFast(LayoutNode node, int value)
        {
            ref Data data = ref _data;
            ref SharedData innerData = ref data.SharedData;
            DebugHelper.ThrowIf(innerData.FakeLayoutNodeKeys is not null);
            DebugHelper.ThrowIf(innerData.FakeLayoutNodeValues is not null);
            DebugHelper.ThrowIf(data.FakeLayoutNodeValuesLength != 0);

            ArrayPool<LayoutNode> nodePool = ArrayPool<LayoutNode>.Shared;
            NativeMemoryPool memoryPool = NativeMemoryPool.Shared;

            LayoutNode[] keys = nodePool.Rent(1);
            innerData.FakeLayoutNodeKeys = keys;

            DebugHelper.ThrowIf(keys.Length < 1);

            TypedNativeMemoryBlock<int> memoryBlock = memoryPool.Rent<int>(1);
            data.MemoryPool = memoryPool;

            int* values = memoryBlock.NativePointer;
            nuint valuesLength = memoryBlock.Length;

            DebugHelper.ThrowIf(valuesLength < 1);

            innerData.FakeLayoutNodeValues = values;
            data.FakeLayoutNodeValuesLength = valuesLength;

            keys.AsUnsafeRef()[0] = node;
            values[0] = value;

            data.NodePool = nodePool;
            innerData.FakeLayoutNodeCount = 1;
        }

        private void SetFakeNodeValueFast(FractionalLayoutNode node, float value)
        {
            ref Data data = ref _data;
            ref SharedData innerData = ref data.SharedData;

            DebugHelper.ThrowIf(innerData.FakeFractionalLayoutNodeKeys is not null);
            DebugHelper.ThrowIf(innerData.FakeFractionalLayoutNodeValues is not null);
            DebugHelper.ThrowIf(data.FakeFractionalLayoutNodeValuesLength != 0);

            ArrayPool<FractionalLayoutNode> nodePool = ArrayPool<FractionalLayoutNode>.Shared;
            NativeMemoryPool memoryPool = NativeMemoryPool.Shared;

            FractionalLayoutNode[] keys = nodePool.Rent(1);
            innerData.FakeFractionalLayoutNodeKeys = keys;

            DebugHelper.ThrowIf(keys.Length < 1);

            TypedNativeMemoryBlock<float> memoryBlock = memoryPool.Rent<float>(1);
            data.MemoryPool = memoryPool;

            float* values = memoryBlock.NativePointer;
            nuint valuesLength = memoryBlock.Length;

            DebugHelper.ThrowIf(valuesLength < 1);

            innerData.FakeFractionalLayoutNodeValues = values;
            data.FakeFractionalLayoutNodeValuesLength = valuesLength;

            keys.AsUnsafeRef()[0] = node;
            values[0] = value;

            data.FractionalNodePool = nodePool;
            innerData.FakeFractionalLayoutNodeCount = 1;
        }

        private void SetFakeNodeValueSlow(LayoutNode node, int value, int count)
        {
            ref Data data = ref _data;
            ref SharedData innerData = ref data.SharedData;

            LayoutNode[]? keys = innerData.FakeLayoutNodeKeys;
            int* values = innerData.FakeLayoutNodeValues;
            nuint valuesLength = data.FakeLayoutNodeValuesLength;

            DebugHelper.ThrowIf(keys is null || keys.Length < count);
            DebugHelper.ThrowIf(values is null);
            DebugHelper.ThrowIf(valuesLength < (uint)count);

            int indexOf = Array.IndexOf(keys, node, 0, count);
            if (indexOf >= 0 && indexOf < count)
            {
                values[indexOf] = value;
                return;
            }

            int newCount = count + 1;
            if (keys.Length < newCount)
            {
                ArrayPool<LayoutNode>? nodePool = data.NodePool;
                DebugHelper.ThrowIf(nodePool is null);

                LayoutNode[] newKeys = nodePool.Rent(newCount);
                DebugHelper.ThrowIf(newKeys.Length < newCount);
                Array.Copy(keys, newKeys, keys.Length);

                nodePool.Return(keys);
                innerData.FakeLayoutNodeKeys = keys = newKeys;
            }
            if (valuesLength < (nuint)newCount)
            {
                NativeMemoryPool? memoryPool = data.MemoryPool;
                DebugHelper.ThrowIf(memoryPool is null);

                TypedNativeMemoryBlock<int> memoryBlock = memoryPool.Rent<int>((nuint)newCount);

                int* newValues = memoryBlock.NativePointer;
                nuint newValuesLength = memoryBlock.Length;
                DebugHelper.ThrowIf(memoryBlock.Length < (nuint)newCount);

                UnsafeHelper.CopyBlockUnaligned(newValues, values, valuesLength);
                memoryPool.Return(new TypedNativeMemoryBlock<int>(values, valuesLength));

                innerData.FakeLayoutNodeValues = values = newValues;
                data.FakeLayoutNodeValuesLength = valuesLength = newValuesLength;
            }

            keys.AsUnsafeRef()[count] = node;
            values[count] = value;
            innerData.FakeLayoutNodeCount = newCount;
        }

        private void SetFakeNodeValueSlow(FractionalLayoutNode node, float value, int count)
        {
            ref Data data = ref _data;
            ref SharedData innerData = ref data.SharedData;

            FractionalLayoutNode[]? keys = innerData.FakeFractionalLayoutNodeKeys;
            float* values = innerData.FakeFractionalLayoutNodeValues;
            nuint valuesLength = data.FakeFractionalLayoutNodeValuesLength;

            DebugHelper.ThrowIf(keys is null || keys.Length < count);
            DebugHelper.ThrowIf(values is null);
            DebugHelper.ThrowIf(valuesLength < (uint)count);

            int indexOf = Array.IndexOf(keys, node, 0, count);
            if (indexOf >= 0 && indexOf < count)
            {
                values[indexOf] = value;
                return;
            }

            int newCount = count + 1;
            if (keys.Length < newCount)
            {
                ArrayPool<FractionalLayoutNode>? nodePool = data.FractionalNodePool;
                DebugHelper.ThrowIf(nodePool is null);

                FractionalLayoutNode[] newKeys = nodePool.Rent(newCount);
                DebugHelper.ThrowIf(newKeys.Length < newCount);
                Array.Copy(keys, newKeys, keys.Length);

                nodePool.Return(keys);
                innerData.FakeFractionalLayoutNodeKeys = keys = newKeys;
            }
            if (valuesLength < (nuint)newCount)
            {
                NativeMemoryPool? memoryPool = data.MemoryPool;
                DebugHelper.ThrowIf(memoryPool is null);

                TypedNativeMemoryBlock<float> memoryBlock = memoryPool.Rent<float>((nuint)newCount);

                float* newValues = memoryBlock.NativePointer;
                nuint newValuesLength = memoryBlock.Length;
                DebugHelper.ThrowIf(memoryBlock.Length < (nuint)newCount);

                UnsafeHelper.CopyBlockUnaligned(newValues, values, valuesLength);
                memoryPool.Return(new TypedNativeMemoryBlock<float>(values, valuesLength));

                innerData.FakeFractionalLayoutNodeValues = values = newValues;
                data.FakeFractionalLayoutNodeValuesLength = valuesLength = newValuesLength;
            }

            keys.AsUnsafeRef()[count] = node;
            values[count] = value;
            innerData.FakeFractionalLayoutNodeCount = newCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            _arguments = default;
            _data.Dispose();
        }
    }
}

public static class VirtualLayoutContextBuilderExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref VirtualLayoutContext.Builder WithFakeNodeValue(this ref VirtualLayoutContext.Builder builder, LayoutNode node, int value)
    {
        builder.SetFakeNodeValue(node, value);
        return ref builder;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref VirtualLayoutContext.Builder WithFakeNodeValue(this ref VirtualLayoutContext.Builder builder, FractionalLayoutNode node, float value)
    {
        builder.SetFakeNodeValue(node, value);
        return ref builder;
    }
}
