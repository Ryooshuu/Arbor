using Arbor.Graphics.Textures;

namespace Arbor.Text;

public class TexturedCharacterGlyph : ITexturedCharacterGlyph
{
    public Texture Texture { get; }

    public float XOffset => glyph.XOffset * Scale;
    public float YOffset => glyph.YOffset * Scale;
    public float XAdvance => glyph.XAdvance * Scale;
    public float Baseline => glyph.Baseline * Scale;
    public char Character => glyph.Character;
    public float Width => Texture.Width * Scale;
    public float Height => Texture.Height * Scale;

    public readonly float Scale;

    private readonly CharacterGlyph glyph;

    public TexturedCharacterGlyph(CharacterGlyph glyph, Texture texture, float scale = 1)
    {
        this.glyph = glyph;
        Scale = scale;
        Texture = texture;
    }

    public float GetKerning<T>(T lastGlyph)
        where T : ICharacterGlyph
        => glyph.GetKerning(lastGlyph) * Scale;
}
