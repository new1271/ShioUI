using ShioUI.Utils;

namespace ShioUI.Traits;

public interface ICursorStateHandler
{
    SystemCursorType? Cursor { get; }
}
