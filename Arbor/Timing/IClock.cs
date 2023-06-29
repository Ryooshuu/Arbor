namespace Arbor.Timing;

public interface IClock
{
    double CurrentTime { get; }
    double Rate { get; }
    bool IsRunning { get; }
}
