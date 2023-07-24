using Arbor.Elements;
using Arbor.Graphics.Textures;
using Arbor.IO.Stores;
using Arbor.Resources;

namespace Arbor;

public abstract class Game : Scene
{
    public static ResourceStore<byte[]> Resources { get; private set; } = null!;

    public TextureStore Textures { get; private set; } = null!;

    internal override void LoadInternal()
    {
        Resources = new ResourceStore<byte[]>();
        Resources.AddStore(new DllResourceStore(ArborResources.ResourcesAssembly));

        Textures = new TextureStore(Pipeline, new TextureLoaderStore(new NamespacedResourceStore<byte[]>(Resources, @"Textures")));
        
        base.LoadInternal();
    }
}
