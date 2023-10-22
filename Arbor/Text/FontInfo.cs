namespace Arbor.Text;

public class FontInfo
{
    public string Family { get; }
    public string Weight { get; }
    public bool Italics { get; }

    public FontInfo(string family, string weight = "Regular", bool italics = false)
    {
        Family = family;
        Weight = weight;
        Italics = italics;
    }
    
    public override string ToString()
        => $"{Family}-{Weight}{(Italics ? "Italic" : string.Empty)}";
}
