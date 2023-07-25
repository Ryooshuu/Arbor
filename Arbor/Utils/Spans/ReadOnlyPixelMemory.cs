using System.Buffers;
using System.Diagnostics;

namespace Arbor.Utils.Spans;

public struct ReadOnlyPixelMemory<TPixel> : IDisposable
    where TPixel : unmanaged, IPixel<TPixel>
{
    private Image<TPixel>? image;
    private Memory<TPixel>? memory;
    private IMemoryOwner<TPixel>? owner;

    internal ReadOnlyPixelMemory(Image<TPixel> image)
    {
        this.image = image;

        if (image.DangerousTryGetSinglePixelMemory(out _))
        {
            owner = null;
            memory = null;
        }
        else
        {
            owner = image.CreateContiguousMemory();
            memory = owner.Memory;
        }
    }

    public ReadOnlySpan<TPixel> Span
    {
        get
        {
            if (image == null)
                return Span<TPixel>.Empty;

            if (image.DangerousTryGetSinglePixelMemory(out var pixelMemory))
                return pixelMemory.Span;
            
            Debug.Assert(memory != null);
            return memory.Value.Span;
        }
    }

    public void Dispose()
    {
        owner?.Dispose();
        image = null;
        memory = null;
        owner = null;
    }
}
