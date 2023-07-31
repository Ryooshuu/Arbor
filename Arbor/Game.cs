using Arbor.Elements;
using Arbor.Graphics.Textures;
using Arbor.IO.Stores;
using Arbor.Platform;
using Arbor.Resources;

namespace Arbor;

public abstract class Game : Scene
{
    public static ResourceStore<byte[]> Resources { get; private set; } = null!;
    public static TextureStore Textures { get; private set; } = null!;
    public static NativeStorage Storage { get; internal set; } = null!;

    internal override void LoadInternal()
    {
        Storage = new NativeStorage(Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.Personal)));
        
        Resources = new ResourceStore<byte[]>();
        Resources.AddStore(new DllResourceStore(ArborResources.ResourcesAssembly));

        Textures = new TextureStore(Pipeline, new TextureLoaderStore(new NamespacedResourceStore<byte[]>(Resources, @"Textures")));
        
        base.LoadInternal();
    }
}
