using GlmSharp;
using Veldrid;

namespace Arbor.Graphics.Textures;

public class Texture : IDisposable
{
    internal virtual Veldrid.Texture NativeTexture { get; }

    private TextureView? textureView;

    internal TextureView TextureView
    {
        get
        {
            if (textureView != null)
                return textureView;
            
            textureView = pipeline.Factory.CreateTextureView(NativeTexture);
            return textureView;
        }
    }

    public string AssetName = string.Empty;

    internal string LookupKey = string.Empty;

    public virtual uint Width { get; set; }
    public virtual uint Height { get; set; }

    public vec2 Size => new(Width, Height);

    internal uint? MipLevel => NativeTexture.MipLevels;

    private readonly DevicePipeline pipeline;

    internal Texture(Veldrid.Texture nativeTexture, DevicePipeline pipeline)
    {
        this.pipeline = pipeline;
        NativeTexture = nativeTexture ?? throw new ArgumentNullException(nameof(nativeTexture));
        
        Width = nativeTexture.Width;
        Height = nativeTexture.Height;
    }

    public Texture(Texture parent)
        : this(parent.NativeTexture, parent.pipeline)
    {
        IsAtlasTexture = parent.IsAtlasTexture;
    }

    internal virtual void SetData(ITextureUpload upload)
    {
        var texture = NativeTexture;

        if (texture.Width != Width || texture.Height != Height)
        {
            texture.Dispose();

            var description = TextureDescription.Texture2D(Width, Height, 0, 1, PixelFormat.R8_G8_B8_A8_UNorm,
                TextureUsage.Sampled | TextureUsage.RenderTarget | TextureUsage.GenerateMipmaps);
            texture = pipeline.Factory.CreateTexture(ref description);
        }

        if (!upload.Data.IsEmpty)
        {
            pipeline.UpdateTexture(texture, upload);
        }
    }

    public virtual RectangleF GetTextureRect(RectangleF? area = null)
        => area ?? new RectangleF(0, 0, Width, Height);

    #region TextureVisualizer Support

    internal bool IsAtlasTexture { get; set; }

    #endregion

    #region Disposal

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool isDisposing)
    {
        NativeTexture?.Dispose();
        textureView?.Dispose();
    }

    #endregion

}
