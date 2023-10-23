using Arbor.Debugging;
using Arbor.Elements;
using Arbor.Graphics;
using Arbor.Graphics.Textures;
using Arbor.IO.Stores;
using Arbor.Platform;
using Arbor.Resources;
using Arbor.Timing;

namespace Arbor;

public abstract class Game : Scene
{
    public static ResourceStore<byte[]> Resources { get; private set; } = null!;
    public static TextureStore Textures { get; private set; } = null!;
    public static NativeStorage Storage { get; internal set; } = null!;
    public static FontStore Fonts { get; internal set; } = null!;

    private FontStore localFonts = null!;


    internal override void LoadInternal()
    {
        Storage = new NativeStorage(Path.GetFullPath(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Window.Title)));

        Resources = new ResourceStore<byte[]>();
        Resources.AddStore(new DllResourceStore(ArborResources.ResourcesAssembly));

        Textures = new TextureStore(Pipeline, new TextureLoaderStore(new NamespacedResourceStore<byte[]>(Resources, @"Textures")));

        var cacheStorage = Storage.GetStorageForDirectory("cached");
        var fontStorage = cacheStorage.GetStorageForDirectory("fonts");

        Fonts = new FontStore(Pipeline, useAtlas: true, cacheStorage: fontStorage);

        Fonts.AddStore(localFonts = new FontStore(Pipeline, useAtlas: false));

        // Roboto
        addFont(localFonts, Resources, @"Fonts/Roboto/Roboto-Regular");
        addFont(localFonts, Resources, @"Fonts/Roboto/Roboto-RegularItalic");
        addFont(localFonts, Resources, @"Fonts/Roboto/Roboto-Bold");
        addFont(localFonts, Resources, @"Fonts/Roboto/Roboto-BoldItalic");

        var debugEntity = CreateEntity();
        debugEntity.AddComponent<TextureVisualiserComponent>();
        debugEntity.AddComponent<StatisticsComponent>();

        base.LoadInternal();
    }

    public void AddFont(ResourceStore<byte[]> store, string? assetName = null, FontStore? target = null)
        => addFont(target ?? Fonts, store, assetName);

    private void addFont(FontStore target, ResourceStore<byte[]> store, string? assetName = null)
        => target.AddTextureSource(new RawCachingGlyphStore(store, assetName, new TextureLoaderStore(store)));

    internal override void UpdateInternal(IFrameBasedClock clock)
    {
        base.UpdateInternal(clock);
        propagateDebugComponents(c => c.Update(clock));   
    }

    internal override void DrawInternal(DrawPipeline pipeline)
    {
        base.DrawInternal(pipeline);
        propagateDebugComponents(c => c.Draw(pipeline));
    }

    private void propagateDebugComponents(Action<IDebugComponent> action)
    {
        foreach (var entity in Entities)
        {
            var components = entity.Components.Where(c => c is IDebugComponent);
            foreach (var c in components)
            {
                action((IDebugComponent)c);
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        Fonts.Dispose();
        localFonts.Dispose();
    }
}
