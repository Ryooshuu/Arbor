using System.Buffers;
using Arbor.Utils.Spans;
using SixLabors.ImageSharp.Advanced;

namespace Arbor.Utils;

public static class ImageExtensions
{
    public static ReadOnlyPixelMemory<TPixel> CreateReadonlyPixelMemory<TPixel>(this Image<TPixel> image)
        where TPixel : unmanaged, IPixel<TPixel> => new ReadOnlyPixelMemory<TPixel>(image);

    internal static IMemoryOwner<TPixel> CreateContiguousMemory<TPixel>(this Image<TPixel> image)
        where TPixel : unmanaged, IPixel<TPixel>
    {
        var allocatedOwner = Configuration.Default.MemoryAllocator.Allocate<TPixel>(image.Width * image.Height);
        var allocatedMemory = allocatedOwner.Memory;

        for (var r = 0; r < image.Height; r++)
            image.DangerousGetPixelRowMemory(r).CopyTo(allocatedMemory.Slice(r * image.Width));

        return allocatedOwner;
    }
}
