using Arbor.IO.Stores;
using Veldrid.ImageSharp;

namespace Arbor.Graphics.Textures;

public class TextureLoaderStore : IResourceStore<ImageSharpTexture>
{
    private readonly ResourceStore<byte[]> store;

    public TextureLoaderStore(IResourceStore<byte[]> store)
    {
        this.store = new ResourceStore<byte[]>(store);
        this.store.AddExtension(@"png");
        this.store.AddExtension(@"jpg");
    }

    public Task<ImageSharpTexture?> GetAsync(string name, CancellationToken cancellationToken = default)
        => Task.Run(() => Get(name), cancellationToken);

    public ImageSharpTexture? Get(string name)
    {
        try
        {
            using (var stream = store.GetStream(name))
            {
                if (stream != null)
                    return new ImageSharpTexture(stream);
            }
        }
        catch
        {
            // ignored
        }

        return null;
    }

    public Stream? GetStream(string name)
        => store.GetStream(name);

    public IEnumerable<string> GetAvailableResources()
        => store.GetAvailableResources();

    #region IDisposable Support

    private bool isDisposed;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (isDisposed)
            return;

        store.Dispose();

        isDisposed = true;
    }

    #endregion
}
