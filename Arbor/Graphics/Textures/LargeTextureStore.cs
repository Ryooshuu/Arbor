using System.Diagnostics;
using Arbor.IO.Stores;

namespace Arbor.Graphics.Textures;

public class LargeTextureStore : TextureStore
{
    private readonly object referenceCountLock = new object();
    private readonly Dictionary<string, TextureWithRefCount.ReferenceCount> referenceCounts = new Dictionary<string, TextureWithRefCount.ReferenceCount>();
    
    public LargeTextureStore(DevicePipeline pipeline, IResourceStore<TextureUpload>? store = null)
        : base(pipeline, store, false)
    {
    }

    protected override bool TryGetCached(string lookupKey, out Texture? texture)
    {
        lock (referenceCountLock)
        {
            if (base.TryGetCached(lookupKey, out var tex))
            {
                texture = createTextureWithRefCount(lookupKey, tex);
                return true;
            }

            texture = null;
            return false;
        }
    }

    protected override Texture? CacheAndReturnTexture(string lookupKey, Texture? texture)
    {
        lock (referenceCountLock)
            return createTextureWithRefCount(lookupKey, base.CacheAndReturnTexture(lookupKey, texture));
    }

    private TextureWithRefCount? createTextureWithRefCount(string lookupKey, Texture? baseTexture)
    {
        if (baseTexture == null)
            return null;

        lock (referenceCountLock)
        {
            if (!referenceCounts.TryGetValue(lookupKey, out var count))
                referenceCounts[lookupKey] = count = new TextureWithRefCount.ReferenceCount(referenceCountLock, () => onAllReferencesLost(baseTexture));

            return new TextureWithRefCount(baseTexture, count);
        }
    }

    private void onAllReferencesLost(Texture texture)
    {
        Debug.Assert(Monitor.IsEntered(referenceCountLock));

        referenceCounts.Remove(texture.LookupKey);
        Purge(texture);
    }
}
