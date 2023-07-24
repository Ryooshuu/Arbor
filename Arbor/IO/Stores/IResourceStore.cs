namespace Arbor.IO.Stores;

public interface IResourceStore<T> : IDisposable
    where T : class
{
    /// <summary>
    /// Retrieves an object from the store.
    /// </summary>
    /// <param name="name">The name of the object.</param>
    /// <returns>The object.</returns>
    T? Get(string name);

    /// <summary>
    /// Retrieves an object from the store asynchronously.
    /// </summary>
    /// <param name="name">The name of the object.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The object.</returns>
    Task<T?> GetAsync(string name, CancellationToken cancellationToken = default);

    Stream? GetStream(string name);

    /// <summary>
    /// Gets a collection of string representations of the resources available in this store.
    /// </summary>
    /// <returns>String representations of the resources available.</returns>
    IEnumerable<string> GetAvailableResources();
}

public static class ResourceStoreExtensions
{
    private static readonly string[] system_filename_ignore_list =
    {
        // Mac
        "__MACOSX",
        ".DS_Store",
        // Windows
        "Thumbs.db"
    };

    public static IEnumerable<string> ExcludeSystemFileNames(this IEnumerable<string> source)
        => source.Where(entry => !system_filename_ignore_list.Any(ignoredName => entry.Contains(ignoredName, StringComparison.OrdinalIgnoreCase)));
}
