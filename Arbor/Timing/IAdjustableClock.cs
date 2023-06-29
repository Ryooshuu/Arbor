namespace Arbor.Timing;

public interface IAdjustableClock : IClock
{
    void Reset();
    void Start();
    void Stop();
    bool Seek(double position);
    new double Rate { get; set; }
    void ResetSpeedAdjustments();
}
