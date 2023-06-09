namespace Arbor.Utils;

public static class ArrayExtensions
{
    public static T[] AddToImmutableArray<T>(IEnumerable<T> values, T value)
    {
        var list = values.ToList();
        list.Add(value);

        return list.ToArray();
    }

    public static T[] AddToImmutableArray<T>(IEnumerable<T> values, T[] value)
    {
        var list = values.ToList();
        list.AddRange(value);

        return list.ToArray();
    }
}
