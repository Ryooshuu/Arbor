using Arbor.Text;

namespace Arbor.IO.Stores;

public interface IGlyphStore : IResourceStore<CharacterGlyph>
{
    string FontName { get; }
    float? Baseline { get; }

    Task LoadFontAsync();
    
    bool HasGlyph(char c);
    
    CharacterGlyph? Get(char character);
    
    int GetKerning(char left, char right);
}
