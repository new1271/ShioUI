using System.Collections.Generic;

using ShioUI.Traits;

namespace ShioUI.Controls.Traits;

public interface IAppendableElementContainer : IElementContainer
{
    UIElement? FirstChild { get; }
    UIElement? LastChild { get; }

    void AddChild(UIElement element);

    void AddChildren(IEnumerable<UIElement> elements);

    void AddChildren(params UIElement[] elements);

    void RemoveChild(UIElement element);
}