using Arbor.Graphics.Textures;

namespace Arbor.Text;

public interface ITexturedCharacterGlyph : ICharacterGlyph
{
    Texture Texture { get; }
    float Width { get; }
    float Height { get; }
}

public static class TexturedCharacterGlyphExtensions
{
    public static bool IsWhiteSpace<T>(this T glyph)
        where T : ITexturedCharacterGlyph
        => char.IsWhiteSpace(glyph.Character);
}
