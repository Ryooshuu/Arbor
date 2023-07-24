using System.Diagnostics;
using Arbor.IO.Stores;
using Arbor.Utils;
using Veldrid;
using Veldrid.ImageSharp;

namespace Arbor.Graphics.Textures;

public class TextureStore : IResourceStore<Texture>
{
    private readonly Dictionary<string, Texture?> textureCache = new();
    private readonly ResourceStore<ImageSharpTexture> sharpStore = new();
    private readonly List<IResourceStore<Texture>> nestedStores = new();

    private readonly DevicePipeline pipeline;

    public TextureStore(DevicePipeline pipeline, IResourceStore<ImageSharpTexture>? store = null)
    {
        if (store != null)
            AddTextureSource(store);

        this.pipeline = pipeline;
    }

    public virtual void AddTextureSource(IResourceStore<ImageSharpTexture> store)
        => sharpStore.AddStore(store);

    public virtual void RemoveTextureSource(IResourceStore<ImageSharpTexture> store)
        => sharpStore.RemoveStore(store);

    public virtual void AddStore(IResourceStore<Texture> store)
    {
        lock (nestedStores)
            nestedStores.Add(store);
    }

    public virtual void RemoveStore(IResourceStore<Texture> store)
    {
        lock (nestedStores)
            nestedStores.Remove(store);
    }

    public Task<Texture?> GetAsync(string name, CancellationToken cancellationToken = default)
        => Task.Run(() => Get(name), cancellationToken);
    
    public virtual Texture? Get(string name)
    {
        var texture = get(name);

        if (texture == null)
        {
            lock (nestedStores)
            {
                foreach (var nested in nestedStores)
                {
                    if ((texture = nested.Get(name)) != null)
                        break;
                }
            }
        }

        return texture;
    }

    public Stream? GetStream(string name)
    {
        var stream = sharpStore.GetStream(name);

        if (stream == null)
        {
            lock (nestedStores)
            {
                foreach (var nested in nestedStores)
                {
                    if ((stream = nested.GetStream(name)) != null)
                        break;
                }
            }
        }

        return stream;
    }

    private Texture loadRaw(ImageSharpTexture upload)
        => pipeline.CreateTexture(upload);

    public IEnumerable<string> GetAvailableResources()
    {
        lock (nestedStores)
            return sharpStore.GetAvailableResources().Concat(nestedStores.SelectMany(s => s.GetAvailableResources()).ExcludeSystemFileNames()).ToArray();
    }

    private readonly Dictionary<string, Task?> retrievalCompletionSources = new();
    
    private Texture? get(string name)
    {
        if (string.IsNullOrEmpty(name))
            return null;

        TaskCompletionSource<Texture>? tcs = null;
        Task? task;

        lock (retrievalCompletionSources)
        {
            if (TryGetCached(name, out var cached))
                return cached;

            if (!retrievalCompletionSources.TryGetValue(name, out task))
                retrievalCompletionSources[name] = (tcs = new TaskCompletionSource<Texture>()).Task;
        }

        if (task != null)
        {
            task.WaitSafely();

            if (TryGetCached(name, out var cached))
                return cached;

            return null;
        }

        Texture? tex = null;

        try
        {
            tex = loadRaw(sharpStore.Get(name)!);

            return CacheAndReturnTexture(name, tex);
        }
        finally
        {
            lock (retrievalCompletionSources)
            {
                Debug.Assert(tcs != null);
                
                tcs.SetResult(tex!);
                retrievalCompletionSources.Remove(name);
            }
        }

        return null;
    }
    
    protected virtual bool TryGetCached(string lookupKey, out Texture? texture)
    {
        lock (textureCache)
            return textureCache.TryGetValue(lookupKey, out texture);
    }

    protected virtual Texture? CacheAndReturnTexture(string lookupKey, Texture? texture)
    {
        lock (textureCache)
            return textureCache[lookupKey] = texture;
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

            sharpStore.Dispose();
            lock (nestedStores)
                nestedStores.ForEach(s => s.Dispose());
        }
    }

    #endregion

}
