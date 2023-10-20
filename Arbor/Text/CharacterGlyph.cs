using Arbor.IO.Stores;

namespace Arbor.Text;

public class CharacterGlyph : ICharacterGlyph
{
    public float XOffset { get; }
    public float YOffset { get; }
    public float XAdvance { get; }
    public float Baseline { get; }
    public char Character { get; }

    private readonly IGlyphStore? containingStore;

    public CharacterGlyph(char character, float xOffset, float yOffset, float xAdvance, float baseline, IGlyphStore? containingStore)
    {
        this.containingStore = containingStore;

        XOffset = xOffset;
        YOffset = yOffset;
        XAdvance = xAdvance;
        Baseline = baseline;
        Character = character;
    }

    public float GetKerning<T>(T lastGlyph)
        where T : ICharacterGlyph
        => containingStore?.GetKerning(lastGlyph.Character, Character) ?? 0;
}
