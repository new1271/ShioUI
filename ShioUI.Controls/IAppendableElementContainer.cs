using System.Collections.Generic;

namespace ShioUI.Controls
{
    public interface IAppendableElementContainer : IElementContainer
    {
        UIElement? FirstChild { get; }
        UIElement? LastChild { get; }

        void AddChild(UIElement element);

        void AddChildren(IEnumerable<UIElement> elements);

        void AddChildren(params UIElement[] elements);

        void RemoveChild(UIElement element);
    }
}