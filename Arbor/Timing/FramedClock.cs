namespace Arbor.Timing;

public class FramedClock : IFrameBasedClock, ISourceChangeableClock
{
    private readonly bool processSource;
    public IClock Source { get; private set; }

    public FramedClock(IClock? source = null, bool processSource = true)
    {
        this.processSource = processSource;
        Source = source ?? new StopwatchClock(true);
        
        ChangeSource(Source);
    }

    public FrameTimeInfo TimeInfo => new FrameTimeInfo
        { Elapsed = ElapsedFrameTime, Current = CurrentTime };
    
    private readonly double[] betweenFrameTimes = new double[128];
    private long totalFramesProcessed;
    
    public double FramesPerSecond { get; private set; }
    public double Jitter { get; private set; }
    public virtual double CurrentTime { get; protected set; }
    protected virtual double LastFrameTime { get; set; }

    public double Rate => Source.Rate;
    protected double SourceTime => Source.CurrentTime;
    public double ElapsedFrameTime => CurrentTime - LastFrameTime;

    public bool IsRunning => Source.IsRunning;
    
    private double timeUntilNextCalculation;
    private double timeSinceLastCalculation;

    private int framesSinceLastCalculation;

    private const int fps_calculation_interval = 250;

    public void ChangeSource(IClock? source)
    {
        if (source == null) return;

        CurrentTime = LastFrameTime = source.CurrentTime;
        Source = source;
    }

    public virtual void ProcessFrame()
    {
        betweenFrameTimes[totalFramesProcessed % betweenFrameTimes.Length] = CurrentTime - LastFrameTime;
        totalFramesProcessed++;

        if (processSource && Source is IFrameBasedClock framedSource)
            framedSource.ProcessFrame();

        if (timeUntilNextCalculation <= 0)
        {
            timeUntilNextCalculation += fps_calculation_interval;

            if (framesSinceLastCalculation == 0)
            {
                FramesPerSecond = 0;
                Jitter = 0;
            }
            else
            {
                FramesPerSecond = (int)Math.Ceiling(framesSinceLastCalculation * 1000f / timeSinceLastCalculation);

                var avg = betweenFrameTimes.Average();
                var stddev = Math.Sqrt(betweenFrameTimes.Average(v => Math.Pow(v - avg, 2)));
                Jitter = stddev;
            }

            timeSinceLastCalculation = framesSinceLastCalculation = 0;
        }

        framesSinceLastCalculation++;
        timeUntilNextCalculation -= ElapsedFrameTime;
        timeSinceLastCalculation += ElapsedFrameTime;

        LastFrameTime = CurrentTime;
        CurrentTime = SourceTime;
    }
    
    public override string ToString() => $@"{GetType().Name} ({Math.Truncate(CurrentTime)}ms, {FramesPerSecond} FPS, ± {Jitter:N2}ms)";
}
