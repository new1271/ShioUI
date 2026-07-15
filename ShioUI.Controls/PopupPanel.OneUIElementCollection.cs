using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

using RiceTea.Core;
using RiceTea.Core.Helpers;
using RiceTea.Core.Threading;

namespace ShioUI.Controls;

public sealed partial class PopupPanel
{
    private sealed class OneUIElementCollection : IReadOnlyCollection<UIElement>, ILockable, ICheckableDisposable
    {
        private readonly PopupPanel _owner;
        private readonly Lock _lock = new Lock();

        private UIElement? _element;
        private bool _disposed;

        public bool IsDisposed
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _disposed;
        }

        public OneUIElementCollection(PopupPanel owner) => _owner = owner;

        public Lock.Scope EnterLockScope() => _lock.EnterScope();

        public int Count => MathHelper.BooleanToInt32(InterlockedHelper.Read(ref _element) is null);

        public UIElement? Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => InterlockedHelper.Read(ref _element);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                using var scope = EnterLockScope();
                UIElement? oldElement = _element;
                if (oldElement == value)
                {
                    oldElement?.Parent = _owner.RootWindow;
                    return;
                }
                _element = value;
                if (value is not null)
                    value.Parent = _owner;
            }
        }

        public IEnumerator<UIElement> GetEnumerator()
        {
            using var scope = EnterLockScope();
            UIElement? element = _element;
            if (element is null)
                return CollectionHelper.EmptyEnumerator<UIElement>();
            return new Enumerator(element);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DisposeCore()
        {
            if (ReferenceHelper.Exchange(ref _disposed, true))
                return;
            using var scope = EnterLockScope();
            DisposeHelper.SwapDisposeWeak(ref _element);
        }

        public void Dispose()
        {
            DisposeCore();
            GC.SuppressFinalize(this);
        }

        private sealed class Enumerator : IEnumerator<UIElement>
        {
            private readonly UIElement _element;

            private int _index;

            public Enumerator(UIElement element)
            {
                _element = element;
                _index = -1;
            }

            public UIElement Current
            {
                get
                {
                    if (_index == 0)
                        return _element;
                    throw new InvalidOperationException();
                }
            }

            object IEnumerator.Current => Current;

            public void Dispose() { }

            public bool MoveNext()
            {
                int index = _index;
                if (index == -1)
                {
                    _index = index + 1;
                    return true;
                }
                index = 1;
                return false;
            }

            public void Reset() => _index = -1;
        }
    }
}
