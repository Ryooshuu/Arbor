using Arbor.IO.Stores;

namespace Arbor.Graphics.Textures;

public class TextureLoaderStore : IResourceStore<TextureUpload>
{
    private readonly ResourceStore<byte[]> store;

    public TextureLoaderStore(IResourceStore<byte[]> store)
    {
        this.store = new ResourceStore<byte[]>(store);
        this.store.AddExtension(@"png");
        this.store.AddExtension(@"jpg");
    }

    public Task<TextureUpload?> GetAsync(string name, CancellationToken cancellationToken = default)
        => Task.Run(() => Get(name), cancellationToken);

    public TextureUpload? Get(string name)
    {
        try
        {
            using (var stream = store.GetStream(name))
            {
                if (stream != null)
                    return new TextureUpload(ImageFromStream<Rgba32>(stream));
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

    protected virtual Image<TPixel> ImageFromStream<TPixel>(Stream stream)
        where TPixel : unmanaged, IPixel<TPixel>
        => TextureUpload.LoadFromStream<TPixel>(stream);

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
