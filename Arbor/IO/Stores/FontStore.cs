using System.Collections.Concurrent;
using Arbor.Graphics;
using Arbor.Graphics.Textures;
using Arbor.Platform;
using Arbor.Text;

namespace Arbor.IO.Stores;

public class FontStore : TextureStore, ITexturedGlyphLookupStore
{
    private readonly List<IGlyphStore> glyphStores = new List<IGlyphStore>();

    private readonly List<FontStore> nestedFontStores = new List<FontStore>();

    private Storage? cacheStorage;

    /*
     * A local cache to avoid string allocation overhead. Can be changed to (string, char) -> string if this ever becomes an issue,
     * but as long as we directly inherit TextureStore this is a slight optimization.
     */
    private readonly ConcurrentDictionary<(string, char), ITexturedCharacterGlyph?> namespacedGlyphCache = new ConcurrentDictionary<(string, char), ITexturedCharacterGlyph?>();

    public FontStore(DevicePipeline pipeline, IResourceStore<TextureUpload>? store = null, float scaleAdjust = 100)
        : this(pipeline, store, scaleAdjust, false)
    {
    }

    internal FontStore(DevicePipeline pipeline, IResourceStore<TextureUpload>? store = null, float scaleAdjust = 100, bool useAtlas = false, Storage? cacheStorage = null)
        : base(pipeline, store, useAtlas, scaleAdjust)
    {
        this.cacheStorage = cacheStorage;
    }

    public override void AddTextureSource(IResourceStore<TextureUpload> store)
    {
        if (store is IGlyphStore gs)
        {
            if (gs is RawCachingGlyphStore { CacheStorage: null } raw)
                raw.CacheStorage = cacheStorage;
            
            glyphStores.Add(gs);
            queueLoad(gs);
        }
        
        base.AddTextureSource(store);
    }

    public override void AddStore(IResourceStore<Texture> store)
    {
        if (store is FontStore fs)
        {
            fs.Atlas ??= Atlas;
            fs.cacheStorage ??= cacheStorage;
            nestedFontStores.Add(fs);
        }
        
        base.AddStore(store);
    }

    private Task? childStoreLoadTasks;

    private void queueLoad(IGlyphStore store)
    {
        var previousLoadStream = childStoreLoadTasks;

        childStoreLoadTasks = Task.Run(async () =>
        {
            if (previousLoadStream != null)
                await previousLoadStream.ConfigureAwait(false);

            try
            {
                Console.WriteLine($"Loading font {store.FontName}...");
                await store.LoadFontAsync().ConfigureAwait(false);
                Console.WriteLine($"Loaded font {store.FontName}!");
            }
            catch
            {
                // ignored
            }
        });
    }

    public override void RemoveTextureSource(IResourceStore<TextureUpload> store)
    {
        if (store is GlyphStore gs)
            glyphStores.Remove(gs);
        
        base.RemoveTextureSource(store);
    }

    public override void RemoveStore(IResourceStore<Texture> store)
    {
        if (store is FontStore fs)
            nestedFontStores.Remove(fs);
        
        base.RemoveStore(store);
    }

    public ITexturedCharacterGlyph? Get(string fontName, char character)
    {
        var key = (fontName, character);

        if (namespacedGlyphCache.TryGetValue(key, out var existing))
            return existing;

        var textureName = string.IsNullOrEmpty(fontName) ? character.ToString() : $"{fontName}/{character}";

        foreach (var store in glyphStores)
        {
            if ((string.IsNullOrEmpty(fontName) || fontName == store.FontName) && store.HasGlyph(character))
                return namespacedGlyphCache[key] = new TexturedCharacterGlyph(store.Get(character)!, Get(textureName)!, 1 / ScaleAdjust);
        }

        foreach (var store in nestedFontStores)
        {
            var glyph = store.Get(fontName, character);
            if (glyph != null)
                return namespacedGlyphCache[key] = glyph;
        }
        
        return namespacedGlyphCache[key] = null;
    }

    public Task<ITexturedCharacterGlyph?> GetAsync(string fontName, char character)
        => Task.Run(() => Get(fontName, character));
}
