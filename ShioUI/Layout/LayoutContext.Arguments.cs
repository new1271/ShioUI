using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ShioUI.Layout;

partial struct LayoutContext
{
    [StructLayout(LayoutKind.Auto)]
    public readonly ref struct Arguments
    {
        public readonly Dictionary<UIElement, ArraySegment<LayoutNode?>> ElementDict;
        public readonly Dictionary<UIElement, ArraySegment<UIElement>> ChildrenDict;
        public readonly Dictionary<UIElement, UIElement> ParentDict;
        public readonly Size PageSize;
        public readonly ulong Timestamp;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Arguments(Dictionary<UIElement, ArraySegment<LayoutNode?>> elementDict, 
            Dictionary<UIElement, ArraySegment<UIElement>> childrenDict, 
            Dictionary<UIElement, UIElement> parentDict, 
            Size pageSize, ulong timestamp)
        {
            ElementDict = elementDict;
            ChildrenDict = childrenDict;
            ParentDict = parentDict;
            PageSize = pageSize;
            Timestamp = timestamp;
        }
    }
}
