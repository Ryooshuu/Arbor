using System.Diagnostics;
using System.Runtime.CompilerServices;
using Arbor.Platform.Windows.Native;

namespace Arbor.Timing;

public class ThrottledFrameClock : FramedClock, IDisposable
{
    public double MaximumUpdateHz = 1000.0;

    public bool Throttling = true;

    public double TimeSlept { get; private set; }

    private IntPtr waitableTimer;

    internal ThrottledFrameClock()
    {
        if (RuntimeInfo.OS == RuntimeInfo.Platform.Windows)
            createWaitableTimer();
    }

    public override void ProcessFrame()
    {
        Debug.Assert(MaximumUpdateHz >= 0);
        base.ProcessFrame();

        if (Throttling)
        {
            if (MaximumUpdateHz > 0 && MaximumUpdateHz < double.MaxValue)
            {
                throttle();
            }
            else
            {
                TimeSlept = sleepAndUpdateCurrent(0);
            }
        }
        else
        {
            TimeSlept = 0;
        }
        
        Debug.Assert(TimeSlept <= ElapsedFrameTime);
    }

    private double accumulatedSleepError;

    private void throttle()
    {
        var excessFrameTime = 1000d / MaximumUpdateHz - ElapsedFrameTime;
        TimeSlept = sleepAndUpdateCurrent(Math.Max(0, excessFrameTime + accumulatedSleepError));

        accumulatedSleepError += excessFrameTime - TimeSlept;
        accumulatedSleepError = Math.Max(-1000 / 30.0, accumulatedSleepError);
    }

    private double sleepAndUpdateCurrent(double milliseconds)
    {
        if (milliseconds <= 0)
            return 0;

        var before = CurrentTime;
        var timeSpan = TimeSpan.FromMilliseconds(milliseconds);
        
        if (!waitWaitableTimer(timeSpan))
            Thread.Sleep(timeSpan);

        return (CurrentTime = SourceTime) - before;
    }

    public void Dispose()
    {
        if (waitableTimer != IntPtr.Zero)
            Execution.CloseHandle(waitableTimer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool waitWaitableTimer(TimeSpan timeSpan)
    {
        if (waitableTimer == IntPtr.Zero) return false;

        if (Execution.SetWaitableTimerEx(waitableTimer, Execution.CreateFileTime(timeSpan), 0, null, default, IntPtr.Zero, 0))
        {
            Execution.WaitForSingleObject(waitableTimer, Execution.INFINITE);
            return true;
        }

        return false;
    }

    private void createWaitableTimer()
    {
        try
        {
            waitableTimer = Execution.CreateWaitableTimerEx(IntPtr.Zero, null,
                Execution.CreateWaitableTimerFlags.CREATE_WAITABLE_TIMER_MANUAL_RESET | Execution.CreateWaitableTimerFlags.CREATE_WAITABLE_TIMER_HIGH_RESOLUTION,
                Execution.TIMER_ALL_ACCESS);

            if (waitableTimer == IntPtr.Zero)
            {
                waitableTimer = Execution.CreateWaitableTimerEx(IntPtr.Zero, null, Execution.CreateWaitableTimerFlags.CREATE_WAITABLE_TIMER_MANUAL_RESET,
                    Execution.TIMER_ALL_ACCESS);
            }
        }
        catch
        {
            // Any kind of unexpected exception should fall back to Thread.Sleep.
        }
    }
}
