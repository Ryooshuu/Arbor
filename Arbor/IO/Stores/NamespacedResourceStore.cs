namespace Arbor.IO.Stores;

public class NamespacedResourceStore<T> : ResourceStore<T>
    where T : class
{
    public readonly string Namespace;

    public NamespacedResourceStore(IResourceStore<T> store, string ns)
        : base(store)
    {
        Namespace = ns;
    }

    protected override IEnumerable<string> GetFilenames(string name)
        => base.GetFilenames($@"{Namespace}/{name}");

    public override IEnumerable<string> GetAvailableResources()
        => base.GetAvailableResources()
           .Where(x => x.StartsWith($"{Namespace}/", StringComparison.Ordinal))
           .Select(x => x[(Namespace.Length + 1)..]);
}
