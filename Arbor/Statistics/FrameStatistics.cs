namespace Arbor.Statistics;

internal static class FrameStatistics
{
    internal static readonly Dictionary<StatisticsCounterType, long> Counts = new Dictionary<StatisticsCounterType, long>();
    internal static readonly List<int> GarbageCollections = new List<int>();
    
    public static double FramesPerSecond { get; set; }

    internal static readonly int NUM_STATISTICS_COUNTER_TYPES = Enum.GetValues(typeof(StatisticsCounterType)).Length;

    internal static readonly long[] COUNTERS = new long[NUM_STATISTICS_COUNTER_TYPES];

    internal static void Clear()
    {
        GarbageCollections.Clear();
        Counts.Clear();
        FramesPerSecond = 0;
    }

    internal static void Increment(StatisticsCounterType type) => ++COUNTERS[(int)type];

    internal static void Add(StatisticsCounterType type, long amount)
    {
        if (amount < 0)
            throw new ArgumentException($"Statistics counter {type} was attempted to be decremented via {nameof(Add)} call.", nameof(amount));

        COUNTERS[(int)type] += amount;
    }

    internal static void Decrement(StatisticsCounterType type, long amount)
    {
        if (amount < 0)
            throw new ArgumentException($"Statistics counter {type} was attempted to be incremented via {nameof(Decrement)} call.", nameof(amount));

        COUNTERS[(int)type] -= amount;
    }
}

internal enum StatisticsCounterType
{
    Buffers = 0,
    VertexMemUsage,
    IndexMemUsage,
    Vertices,
    Indices,
    DrawCalls
}