using System.Buffers;
using SixLabors.ImageSharp.Memory;

namespace Arbor.Graphics.Textures;

public class MemoryAllocatorTextureUpload : ITextureUpload
{
    public Span<Rgba32> RawData => memoryOwner.Memory.Span;

    public ReadOnlySpan<Rgba32> Data => RawData;

    private readonly IMemoryOwner<Rgba32> memoryOwner;
    
    public Rectangle Bounds { get; set; }

    public MemoryAllocatorTextureUpload(int width, int height, MemoryAllocator? memoryAllocator = null)
    {
        memoryOwner = (memoryAllocator ?? Configuration.Default.MemoryAllocator).Allocate<Rgba32>(width * height);
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

        memoryOwner?.Dispose();

        disposed = true;
    }

    #endregion
}
