namespace Arbor.Timing;

public interface ISourceChangeableClock : IClock
{
    IClock? Source { get; }

    void ChangeSource(IClock? source);
}
