namespace Arbor.Graphics.Textures;

public class TextureWhitePixel : TextureRegion
{
    public TextureWhitePixel(Texture texture)
        : base(texture, new Rectangle(0, 0, 1, 1))
    {
    }
}
