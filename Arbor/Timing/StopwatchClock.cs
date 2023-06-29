using System.Diagnostics;

namespace Arbor.Timing;

public class StopwatchClock : Stopwatch, IAdjustableClock
{
    private double seekOffset;
    private double rateChangeUsed;
    private double rateChangeAccumulated;

    public StopwatchClock(bool start = false)
    {
        if (start)
            Start();
    }

    public double CurrentTime => stopwatchCurrentTime + seekOffset;

    private double stopwatchCurrentTime => (stopwatchMilliseconds - rateChangeUsed) * rate + rateChangeAccumulated;

    private double stopwatchMilliseconds => (double) ElapsedTicks / Frequency * 1000;

    private double rate = 1;

    public double Rate
    {
        get => rate;
        set
        {
            if (rate == value) return;

            rateChangeAccumulated += (stopwatchMilliseconds - rateChangeUsed) * rate;
            rateChangeUsed = stopwatchMilliseconds;

            rate = value;
        }
    }

    public new void Reset()
    {
        resetAccumulatedRate();
        base.Reset();
    }
    
    public new void Restart()
    {
        resetAccumulatedRate();
        base.Restart();
    }

    public void ResetSpeedAdjustments()
        => Rate = 1;

    public bool Seek(double position)
    {
        seekOffset = position - stopwatchCurrentTime;
        return true;
    }

    private void resetAccumulatedRate()
    {
        rateChangeAccumulated = 0;
        rateChangeUsed = 0;
    }
}
