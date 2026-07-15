using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using ShioUI.Layout;

namespace ShioUI.Internals;

internal sealed class UIElementEqualityComparer : IEqualityComparer<UIElement>
{
    public static readonly UIElementEqualityComparer Instance = new UIElementEqualityComparer();

    private UIElementEqualityComparer() { }

    public bool Equals(UIElement? x, UIElement? y) => ReferenceEquals(x, y);

    public int GetHashCode([DisallowNull] UIElement obj) => obj.ElementId;
}

internal sealed class LayoutNodeBaseEqualityComparer : IEqualityComparer<LayoutNodeBase>
{
    public static readonly LayoutNodeBaseEqualityComparer Instance = new LayoutNodeBaseEqualityComparer();

    private LayoutNodeBaseEqualityComparer() { }

    public bool Equals(LayoutNodeBase? x, LayoutNodeBase? y) => ReferenceEquals(x, y);

    public int GetHashCode([DisallowNull] LayoutNodeBase obj) => obj.NodeId;
}