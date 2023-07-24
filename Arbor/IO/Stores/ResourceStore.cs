namespace Arbor.IO.Stores;

public class ResourceStore<T> : IResourceStore<T>
    where T : class
{
    private readonly Dictionary<string, Action?> actionList = new();
    private readonly List<IResourceStore<T>> stores = new();
    private readonly List<string> searchExtensions = new();

    public ResourceStore()
    {
    }

    public ResourceStore(IResourceStore<T>? store = null)
    {
        if (store != null)
            AddStore(store);
    }

    public ResourceStore(IResourceStore<T>[] stores)
    {
        foreach (var resourceStore in stores)
            AddStore(resourceStore);
    }

    protected virtual void NotifyChanged(string name)
    {
        if (!actionList.TryGetValue(name, out var action))
            return;

        action?.Invoke();
    }

    public virtual void AddStore(IResourceStore<T> store)
    {
        lock (stores)
            stores.Add(store);
    }

    public virtual void RemoveStore(IResourceStore<T> store)
    {
        lock (stores)
            stores.Remove(store);
    }

    public virtual async Task<T?> GetAsync(string? name, CancellationToken cancellationToken = default)
    {
        if (name == null)
            return null;

        var filenames = GetFilenames(name);

        foreach (var store in getStores())
        {
            foreach (var f in filenames)
            {
                var result = await store.GetAsync(f, cancellationToken).ConfigureAwait(false);

                if (result != null)
                    return result;
            }
        }

        return default;
    }

    public virtual T? Get(string? name)
    {
        if (name == null)
            return null;

        var filenames = GetFilenames(name);

        foreach (var store in getStores())
        {
            foreach (var f in filenames)
            {
                var result = store.Get(f);

                if (result != null)
                    return result;
            }
        }

        return default;
    }

    public Stream? GetStream(string? name)
    {
        if (name == null)
            return null;

        var filenames = GetFilenames(name);

        foreach (var store in getStores())
        {
            foreach (var f in filenames)
            {
                var result = store.GetStream(f);

                if (result != null)
                    return result;
            }
        }

        return null;
    }

    protected virtual IEnumerable<string> GetFilenames(string name)
    {
        yield return name;

        foreach (var ext in searchExtensions)
            yield return $@"{name}.{ext}";
    }

    public void BindReload(string name, Action? onReload)
    {
        if (onReload == null)
            return;

        if (actionList.ContainsKey(name))
            throw new InvalidOperationException($"A reload delegate is already bound to the resource '{name}'.");

        actionList[name] = onReload;
    }

    public void AddExtension(string extension)
    {
        extension = extension.Trim('.');
        
        if (!searchExtensions.Contains(extension))
            searchExtensions.Add(extension);
    }

    public virtual IEnumerable<string> GetAvailableResources()
    {
        lock (stores)
            return stores.SelectMany(s => s.GetAvailableResources()).ExcludeSystemFileNames();
    }

    private IResourceStore<T>[] getStores()
    {
        lock (stores)
            return stores.ToArray();
    }

    #region IDisposable Support

    private bool isDisposed;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!isDisposed)
        {
            isDisposed = true;
            lock (stores)
                stores.ForEach(s => s.Dispose());
        }
    }

    #endregion

}
