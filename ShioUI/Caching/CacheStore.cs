using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

using RiceTea.Core;
using RiceTea.Core.Buffers;
using RiceTea.Core.Helpers;

namespace ShioUI.Caching;

public sealed unsafe partial class CacheStore<T> : IDisposable
{
    private static readonly Pool<Node> _snapshotPool = new(initialLength: 32);

    private readonly Dictionary<ulong, Node> _snapshotDict = new();
    private readonly Lock _syncLock = new();
    private readonly object? _owner;
    private readonly delegate* managed<object?, Body> _createSnapshotFunc;
    private readonly delegate* managed<object?, in Body, void> _removeSnapshotFunc;

    private Node? _lastSnapshot;
    private ulong _lastTimestamp;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public CacheStore(object? owner,
        delegate* managed<object?, Body> createSnapshotFunc,
        delegate* managed<object?, in Body, void> removeSnapshotFunc)
    {
        _owner = owner;
        _createSnapshotFunc = createSnapshotFunc;
        _removeSnapshotFunc = removeSnapshotFunc;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UpdateTimestamp(ulong timestamp)
    {
        lock (_syncLock)
        {
            if (Cells.Exchange(ref _lastTimestamp, timestamp) == timestamp)
                return;
            Node? lastSnapshot = Cells.Exchange(ref _lastSnapshot, null);
            if (lastSnapshot is not null)
                Dereference(lastSnapshot);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Scope GetLastSnapshot()
    {
        Node? snapshot = Atomics.Read(ref _lastSnapshot);
        if (snapshot is not null)
        {
            snapshot.EnterBarrier();
            try
            {
                if (!ReferenceEquals(this, snapshot.Owner))
                    goto Slow;
                snapshot.AddRef();
            }
            finally
            {
                snapshot.ExitBarrier();
            }
            return new Scope(snapshot);
        }

    Slow:
        return new Scope(GetSnapshotSlow());
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private Node GetSnapshotSlow()
    {
        lock (_syncLock)
        {
            Node snapshot = Core();
            snapshot.AddRef();
            _lastSnapshot = snapshot;
            return snapshot;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        Node Core()
        {
            Node? snapshot = _lastSnapshot;
            if (snapshot is not null)
            {
                snapshot.EnterBarrier();
                try
                {
                    if (!ReferenceEquals(this, snapshot.Owner))
                        goto Create;
                    snapshot.AddRef();
                }
                finally
                {
                    snapshot.ExitBarrier();
                }
                return snapshot;
            }

        Create:
            snapshot = _snapshotPool.Rent();

            ulong timestamp = Atomics.Read(ref _lastTimestamp);
            snapshot.Body = _createSnapshotFunc(_owner);

            snapshot.AddRef();
            snapshot.Timestamp = timestamp;
            snapshot.Owner = this;
            _snapshotDict[timestamp] = snapshot;
            return snapshot;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Dereference(Node node)
    {
        node.EnterBarrier();
        try
        {
            if (!ReferenceEquals(this, node.Owner) || node.RemoveRef())
                return;
            lock (_syncLock)
            {
                DebugHelper.ThrowIf(ReferenceEquals(Atomics.Read(ref _lastSnapshot), node));
                ((ICollection<KeyValuePair<ulong, Node>>)_snapshotDict).Remove(KeyValuePair.Create(node.Timestamp, node));
            }
            _removeSnapshotFunc(_owner, node.Body);
            node.CleanUp();
            _snapshotPool.Return(node);
        }
        finally
        {
            node.ExitBarrier();
        }
    }

    public void Dispose()
    {
        lock (_syncLock)
        {
            Node? lastSnapshot = Cells.Exchange(ref _lastSnapshot, null);
            if (lastSnapshot is not null)
            {
                try
                {
                    Dereference(lastSnapshot);
                }
                catch (Exception)
                {
                }
            }
        }
    }
}
