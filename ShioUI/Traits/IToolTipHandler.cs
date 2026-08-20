using System.Diagnostics.CodeAnalysis;
using System.Drawing;

namespace ShioUI.Traits;

public interface IToolTipHandler
{
    bool TryGetToolTipText(PointF location, [NotNullWhen(true)] out string? result);
}
