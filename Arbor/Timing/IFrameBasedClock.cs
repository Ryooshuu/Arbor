namespace Arbor.Timing;

public interface IFrameBasedClock : IClock
{
    double ElapsedFrameTime { get; }
    
    double FramesPerSecond { get; }
    FrameTimeInfo TimeInfo { get; }

    void ProcessFrame();
}
