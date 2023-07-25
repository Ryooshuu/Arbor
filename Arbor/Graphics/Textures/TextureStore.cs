using System.Diagnostics;
using Arbor.IO.Stores;
using Arbor.Utils;

namespace Arbor.Graphics.Textures;

public class TextureStore : IResourceStore<Texture>
{
    private readonly Dictionary<string, Texture?> textureCache = new();
    private readonly ResourceStore<TextureUpload> uploadStore = new();
    private readonly List<IResourceStore<Texture>> nestedStores = new();

    private readonly DevicePipeline pipeline;

    protected readonly TextureAtlas? Atlas;

    private const int max_atlas_size = 1024;

    public TextureStore(DevicePipeline pipeline, IResourceStore<TextureUpload>? store = null, bool useAtlas = true)
    {
        if (store != null)
            AddTextureSource(store);

        this.pipeline = pipeline;

        if (useAtlas)
        {
            Atlas = new TextureAtlas(pipeline, max_atlas_size, max_atlas_size);
        }
    }

    public virtual void AddTextureSource(IResourceStore<TextureUpload> store)
        => uploadStore.AddStore(store);

    public virtual void RemoveTextureSource(IResourceStore<TextureUpload> store)
        => uploadStore.RemoveStore(store);

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
        var stream = uploadStore.GetStream(name);

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

    private Texture? loadRaw(TextureUpload upload)
    {
        Texture? tex = null;

        if (Atlas != null)
        {
            if ((tex = Atlas.Add((int) upload.Width, (int) upload.Height)) == null)
            {
                Console.WriteLine($"Texture requested ({upload.Width}x{upload.Height}) which exceeds {nameof(TextureStore)}'s atlas size ({max_atlas_size}x{max_atlas_size}) - bypassing atlasing.");
            }
        }

        tex ??= pipeline.CreateTexture(upload);
        tex.SetData(upload);

        return tex;
    }

    public IEnumerable<string> GetAvailableResources()
    {
        lock (nestedStores)
            return uploadStore.GetAvailableResources().Concat(nestedStores.SelectMany(s => s.GetAvailableResources()).ExcludeSystemFileNames()).ToArray();
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
            tex = loadRaw(uploadStore.Get(name)!);
            if (tex != null)
                tex.LookupKey = name;

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

    protected void Purge(Texture texture)
    {
        lock (textureCache)
        {
            if (textureCache.TryGetValue(texture.LookupKey, out var tex))
            {
                if (tex != null)
                    new DisposableTexture(tex).Dispose();
            }

            textureCache.Remove(texture.LookupKey);
        }
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

            uploadStore.Dispose();
            lock (nestedStores)
                nestedStores.ForEach(s => s.Dispose());
        }
    }

    #endregion

}
