using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

using RiceTea.Core.Buffers;
using RiceTea.Core.Extensions;
using RiceTea.Core.Helpers;
using RiceTea.Core.Threading;

namespace ShioUI.Utils;

public sealed class UIElementCollection : ICollection, ICollection<UIElement>, ILockable, IDisposable
{
    private readonly HashSet<UIElement> _filters = new HashSet<UIElement>();
    private readonly List<UIElement> _list = new List<UIElement>();
    private readonly Lock _lock = new Lock();
    private readonly IElementContainer _owner;

    private bool _disposed;

    public UIElementCollection(IElementContainer owner) => _owner = owner;

    public int Count
    {
        get
        {
            lock (_lock)
                return _filters.Count;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Lock.Scope EnterLockScope() => _lock.EnterScope();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public UIElement? FirstOrDefault()
    {
        using var scope = EnterLockScope();
        return _list.FirstOrDefault();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public UIElement? LastOrDefault()
    {
        using var scope = EnterLockScope();
        return _list.LastOrDefault();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(UIElement element)
    {
        using var scope = EnterLockScope();
        if (!_filters.Add(element))
            return;
        _list.Add(element);
        element.Parent = _owner;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddRange(params UIElement[] elements)
    {
        int length = elements.Length;
        if (length <= 0)
            return;

        using var scope = EnterLockScope();
        IElementContainer owner = _owner;
        HashSet<UIElement> filters = _filters;
        List<UIElement> list = _list;
        ref readonly UIElement elementsRef = ref UnsafeHelper.GetArrayDataReference(elements);

        int i = 0;
        do
        {
            UIElement element = UnsafeHelper.AddTypedOffsetAsReadOnly(in elementsRef, i);
            if (!filters.Add(element))
                continue;
            list.Add(element);
            element.Parent = owner;
        } while (++i < length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddRange<TElements>(TElements elements) where TElements : IEnumerable<UIElement>
    {
        IElementContainer owner = _owner;

        using ArrayPool<UIElement>.RentScope rentScope = ArrayPool<UIElement>.Shared.EnterRentScopeAndCapture(elements);

        using var scope = EnterLockScope();
        HashSet<UIElement> filters = _filters;
        List<UIElement> list = _list;
        foreach (UIElement element in rentScope)
        {
            if (!filters.Add(element))
                continue;
            list.Add(element);
            element.Parent = owner;
        }
    }

    public void Clear()
    {
        using var scope = EnterLockScope();
        _filters.Clear();
        _list.Clear();
    }

    public bool Contains(UIElement item)
    {
        using var scope = EnterLockScope();
        return _list.Contains(item);
    }

    public void CopyTo(UIElement[] array, int arrayIndex)
    {
        using var scope = EnterLockScope();
        _list.CopyTo(array, arrayIndex);
    }

    public IEnumerator<UIElement> GetEnumerator() => _list.GetEnumerator();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(UIElement element)
    {
        using var scope = EnterLockScope();
        if (!_filters.Remove(element))
            return false;
        bool check = _list.Remove(element);
        DebugHelper.ThrowUnless(check);
        element.Parent = _owner.RootWindow;
        return true;
    }

    bool ICollection<UIElement>.IsReadOnly => false;

    object ICollection.SyncRoot => this;

    bool ICollection.IsSynchronized => false;

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    void ICollection.CopyTo(Array array, int index)
    {
        using var scope = EnterLockScope();
        ((ICollection)_list).CopyTo(array, index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void DisposeCore(bool disposing)
    {
        if (ReferenceHelper.Exchange(ref _disposed, true))
            return;
        if (disposing)
        {
            using var scope = EnterLockScope();
            foreach (UIElement element in _list)
                element.Dispose();
        }
        _filters.Clear();
        _list.Clear();
    }

    ~UIElementCollection() => DisposeCore(disposing: false);

    public void Dispose()
    {
        DisposeCore(disposing: true);
        GC.SuppressFinalize(this);
    }
}
