namespace Arbor.Text;

public interface ITexturedGlyphLookupStore
{
    ITexturedCharacterGlyph? Get(string fontName, char character);

    Task<ITexturedCharacterGlyph?> GetAsync(string fontName, char character);
}
