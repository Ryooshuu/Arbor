namespace Arbor.Text;

public interface ICharacterGlyph
{
    float XOffset { get; }
    float YOffset { get; }
    float XAdvance { get; }
    float Baseline { get; }
    
    char Character { get; }

    float GetKerning<T>(T lastGlyph) where T : ICharacterGlyph;
}
