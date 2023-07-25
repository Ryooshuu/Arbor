namespace Arbor.Graphics.Textures;

public class TextureRegion : Texture
{
    private readonly Texture parent;
    private readonly Rectangle bounds;

    public TextureRegion(Texture parent, Rectangle bounds)
        : base(parent)
    {
        this.parent = parent;
        this.bounds = bounds;
    }

    public override uint Width => (uint) bounds.Width;
    public override uint Height => (uint) bounds.Height;
    
    internal override void SetData(ITextureUpload upload)
    {
        if (upload.Bounds.Width > bounds.Width || upload.Bounds.Height > bounds.Height)
        {
            throw new ArgumentOutOfRangeException(nameof(upload),
                $"Texture is too small to fit the requested upload. Texture size is {bounds.Width} x {bounds.Height}, upload size is {upload.Bounds.Width} x {upload.Bounds.Height}.");
        }

        if (upload.Bounds.IsEmpty)
            upload.Bounds = bounds;
        else
        {
            var adjustedBounds = upload.Bounds;
            
            adjustedBounds.X += bounds.X;
            adjustedBounds.Y += bounds.Y;
            
            upload.Bounds = adjustedBounds;
        }
        
        parent.SetData(upload);
    }
    
    private RectangleF boundsInParent(RectangleF? area)
    {
        RectangleF actualBounds = bounds;

        if (area is { } rect)
        {
            actualBounds.X += rect.X;
            actualBounds.Y += rect.Y;
            actualBounds.Width = rect.Width;
            actualBounds.Height = rect.Height;
        }

        return actualBounds;
    }

    public override RectangleF GetTextureRect(RectangleF? area = null) => parent.GetTextureRect(boundsInParent(area));

    protected override void Dispose(bool isDisposing)
    {
        // We don't want to dispose the parent texture.
    }
}
