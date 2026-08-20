using ShioUI.Graphics.Native.DirectWrite;

namespace ShioUI.Utils;

public static class SharedResources
{
    public static readonly DWriteFactory DWriteFactory = DWriteFactory.Create(DWriteFactoryType.Shared);
}
