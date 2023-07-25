using System.Runtime.InteropServices;
using Arbor.Utils;
using Arbor.Utils.Spans;
using StbiSharp;

namespace Arbor.Graphics.Textures;

public class TextureUpload : ITextureUpload
{
    public Rectangle Bounds { get; set; }

    public ReadOnlySpan<Rgba32> Data => pixelMemory.Span;

    public uint Width => (uint) (image?.Width ?? 0);
    public uint Height => (uint) (image?.Height ?? 0);

    private readonly Image<Rgba32>? image;

    private readonly ReadOnlyPixelMemory<Rgba32> pixelMemory;

    public TextureUpload(Image<Rgba32> image)
    {
        this.image = image;
        pixelMemory = image.CreateReadonlyPixelMemory();
    }

    public TextureUpload(Stream stream)
        : this(LoadFromStream<Rgba32>(stream))
    {
    }

    private static bool stbiNotFound;
    
    internal static Image<TPixel> LoadFromStream<TPixel>(Stream stream)
        where TPixel : unmanaged, IPixel<TPixel>
    {
        if (stbiNotFound)
            return Image.Load<TPixel>(stream);

        var initialPos = stream.Position;

        try
        {
            using var buffer = Configuration.Default.MemoryAllocator.Allocate<byte>((int) stream.Length);
            stream.ReadToFill(buffer.Memory.Span);

            using var stbiImage = Stbi.LoadFromMemory(buffer.Memory.Span, 4);
            return Image.LoadPixelData(MemoryMarshal.Cast<byte, TPixel>(stbiImage.Data), stbiImage.Width, stbiImage.Height);
        }
        catch (Exception e)
        {
            if (e is DllNotFoundException)
                stbiNotFound = true;

            Console.WriteLine($"Texture could not be loaded via STB; falling back to ImageSharp: {e.Message}");
            stream.Position = initialPos;
            return Image.Load<TPixel>(stream);
        }
    }

    internal TextureUpload()
    {
    }

    #region IDisposable Support

    private bool disposed;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool isDisposing)
    {
        if (disposed)
            return;

        image?.Dispose();
        pixelMemory.Dispose();

        disposed = true;
    }

    #endregion
}
