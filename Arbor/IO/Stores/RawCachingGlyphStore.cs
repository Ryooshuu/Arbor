using System.Buffers;
using System.Diagnostics;
using System.Globalization;
using Arbor.Graphics.Textures;
using Arbor.Platform;
using Arbor.Utils;
using SharpFNT;
using SixLabors.ImageSharp.Advanced;

namespace Arbor.IO.Stores;

public class RawCachingGlyphStore : GlyphStore
{
    internal Storage? CacheStorage { get; set; }

    private readonly Dictionary<string, Stream> pageStreamHandles = new Dictionary<string, Stream>();

    private readonly Dictionary<int, PageInfo> pageLookup = new Dictionary<int, PageInfo>();
    
    public RawCachingGlyphStore(ResourceStore<byte[]> store, string? assetName = null, IResourceStore<TextureUpload>? textureLoader = null)
        : base(store, assetName, textureLoader)
    {
    }

    protected override TextureUpload LoadCharacter(Character character)
    {
        if (CacheStorage == null)
            throw new InvalidOperationException($"{nameof(CacheStorage)} should be set before requesting characters.");

        lock (pageLookup)
        {
            if (!pageLookup.TryGetValue(character.Page, out var pageInfo))
                pageInfo = createCachedPageInfo(character.Page);

            return createTextureUpload(character, pageInfo);
        }
    }

    private PageInfo createCachedPageInfo(int page)
    {
        Debug.Assert(CacheStorage != null);
        var filename = GetFilenameForPage(page);

        using (var stream = Store.GetStream(filename)!)
        {
            var streamMd5 = stream.ComputeMD5Hash();
            var filenameMd5 = filename.ComputeMD5Hash();
            
            var accessFilename = $"{filenameMd5}#{streamMd5}";

            var existing = CacheStorage.GetFiles(string.Empty, $"{accessFilename}*").FirstOrDefault();

            if (existing != null)
            {
                var split = existing.Split('#');

                if (split.Length == 4 &&
                    int.TryParse(split[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var width) &&
                    int.TryParse(split[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out var height))
                {
                    using (var testStream = CacheStorage.GetStream(existing)!)
                    {
                        if (testStream.Length == width * height)
                        {
                            return pageLookup[page] = new PageInfo
                            {
                                Size = new Size(width, height),
                                Filename = existing
                            };
                        }
                    }
                }
            }
            
            using (var convert = GetPageImage(page)!)
            using (var buffer = SixLabors.ImageSharp.Configuration.Default.MemoryAllocator.Allocate<byte>((int)(convert.Width * convert.Height)))
            {
                var output = buffer.Memory.Span;
                var source = convert.Data;

                for (var i = 0; i < output.Length; i++)
                    output[i] = source[i].A;
                
                // ensure any stale cached versions are deleted.
                foreach (var f in CacheStorage.GetFiles(string.Empty, $"{filenameMd5}*"))
                    CacheStorage.Delete(f);
                
                accessFilename += FormattableString.Invariant($"#{convert.Width}#{convert.Height}");

                using (var outStream = CacheStorage.CreateFileSafely(accessFilename))
                    outStream.Write(buffer.Memory.Span);

                return pageLookup[page] = new PageInfo
                {
                    Size = new Size((int) convert.Width, (int) convert.Height),
                    Filename = accessFilename
                };
            }
        }
    }

    private TextureUpload createTextureUpload(Character character, PageInfo page)
    {
        Debug.Assert(CacheStorage != null);

        var pageWidth = page.Size.Width;

        var characterByteRegion = pageWidth * character.Height;
        var readBuffer = ArrayPool<byte>.Shared.Rent(characterByteRegion);

        try
        {
            var image = new Image<Rgba32>(SixLabors.ImageSharp.Configuration.Default, character.Width, character.Height);

            if (!pageStreamHandles.TryGetValue(page.Filename, out var source))
                source = pageStreamHandles[page.Filename] = CacheStorage.GetStream(page.Filename)!;

            source.Seek(pageWidth * character.Y, SeekOrigin.Begin);
            source.ReadToFill(readBuffer.AsSpan(0, characterByteRegion));
            
            var readableHeight = Math.Min(character.Height, page.Size.Height - character.Y);
            var readableWidth = Math.Min(character.Width, pageWidth - character.X);
            
            for (var y = 0; y < character.Height; y++)
            {
                var pixelRowMemory = image.DangerousGetPixelRowMemory(y);
                var span = pixelRowMemory.Span;
                var readOffset = y * pageWidth + character.X;

                for (var x = 0; x < character.Width; x++)
                    span[x] = new Rgba32(255, 255, 255, x < readableWidth && y < readableHeight ? readBuffer[readOffset + x] : (byte) 0);
            }

            return new TextureUpload(image);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(readBuffer);
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (!pageStreamHandles.Any())
            return;

        foreach (var h in pageStreamHandles)
            h.Value.Dispose();
    }

    private record PageInfo
    {
        public string Filename { get; set; } = string.Empty;
        public Size Size { get; set; }
    }
}
