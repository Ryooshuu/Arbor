using System.Diagnostics;
using System.Numerics;
using GlmSharp;

namespace Arbor.Graphics.Textures;

public partial class TextureAtlas
{
    internal const uint PADDING = (1 << 3) * 3;
    internal const uint WHITE_PIXEL_SIZE = 1;

    private readonly List<Rectangle> subTextureBounds = new();
    private Texture? atlasTexture;

    private readonly DevicePipeline pipeline;
    private readonly uint atlasWidth;
    private readonly uint atlasHeight;

    private uint maxFittableWidth => atlasWidth - PADDING * 2;
    private uint maxFittableHeight => atlasHeight - PADDING * 2;

    private vec2 currentPosition;

    internal TextureWhitePixel WhitePixel
    {
        get
        {
            if (atlasTexture == null)
                Reset();

            Debug.Assert(atlasTexture != null, "Atlas texture should not be null after Reset().");
            return new TextureWhitePixel(atlasTexture);
        }
    }
    
    private readonly object textureRetrievalLock = new();

    public TextureAtlas(DevicePipeline pipeline, int width, int height)
    {
        this.pipeline = pipeline;
        atlasWidth = (uint) width;
        atlasHeight = (uint) height;
    }

    private int exceedCount;

    public void Reset()
    {
        lock (textureRetrievalLock)
        {
            subTextureBounds.Clear();
            currentPosition = vec2.Zero;

            atlasTexture = new BackingAtlasTexture(pipeline, (int) atlasWidth, (int) atlasHeight, (int) PADDING / 2);

            var bounds = new Rectangle(0, 0, (int) WHITE_PIXEL_SIZE, (int) WHITE_PIXEL_SIZE);
            subTextureBounds.Add(bounds);

            using (var whiteTex = new TextureRegion(atlasTexture, bounds))
                whiteTex.SetData(new TextureUpload(new Image<Rgba32>(Configuration.Default, (int) whiteTex.Width, (int) whiteTex.Height, new Rgba32(Vector4.One))));

            currentPosition = new vec2(PADDING + WHITE_PIXEL_SIZE, PADDING);
        }
    }

    public Texture? Add(int width, int height)
    {
        if (!canFitEmptyTextureAtlas(width, height))
            return null;

        lock (textureRetrievalLock)
        {
            var position = findPosition(width, height);
            Debug.Assert(atlasTexture != null, "Atlas texture should not be null after findPosition().");
            
            var bounds = new Rectangle((int) position.x, (int) position.y, width, height);
            subTextureBounds.Add(bounds);

            return new TextureRegion(atlasTexture, bounds);
        }
    }

    private bool canFitEmptyTextureAtlas(int width, int height)
    {
        if (width > maxFittableWidth || height > maxFittableHeight)
            return false;

        if (width + WHITE_PIXEL_SIZE > maxFittableWidth && height + WHITE_PIXEL_SIZE > maxFittableHeight)
            return false;

        return true;
    }

    private vec2 findPosition(int width, int height)
    {
        if (atlasTexture == null)
        {
            Console.WriteLine($"TextureAtlas initialised ({atlasWidth}x{atlasHeight})");
            Reset();
        }

        if (currentPosition.y + height + PADDING > atlasHeight)
        {
            Console.WriteLine($"TextureAtlas size exceeded {++exceedCount} time(s); generating new texture ({atlasWidth}x{atlasHeight})");
            Reset();
        }

        if (currentPosition.x + width + PADDING > atlasWidth)
        {
            var maxY = 0;

            foreach (var bounds in subTextureBounds)
                maxY = (int) Math.Max(maxY, bounds.Bottom + PADDING);
            
            subTextureBounds.Clear();
            currentPosition = new vec2(PADDING, maxY);

            return findPosition(width, height);
        }

        var result = currentPosition;
        currentPosition.x += width + PADDING;
        
        return result;
    }
}
