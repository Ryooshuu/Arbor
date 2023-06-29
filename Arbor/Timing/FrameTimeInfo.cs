using System.Globalization;

namespace Arbor.Timing;

public struct FrameTimeInfo
{
    public double Elapsed;
    public double Current;

    public override string ToString()
        => Math.Truncate(Current).ToString(CultureInfo.InvariantCulture);
}
