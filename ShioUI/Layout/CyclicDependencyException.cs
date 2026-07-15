using System;

namespace ShioUI.Layout;

public sealed class CyclicDependencyException : Exception
{
    private readonly LayoutNodeBase[] _walkedNodes;

    public LayoutNodeBase[] WalkedNodes => _walkedNodes;

    public CyclicDependencyException(LayoutNodeBase[] walkedNodes)
    {
        _walkedNodes = walkedNodes;
    }

    public CyclicDependencyException(string? message, LayoutNodeBase[] walkedNodes) : base(message)
    {
        _walkedNodes = walkedNodes;
    }
}
