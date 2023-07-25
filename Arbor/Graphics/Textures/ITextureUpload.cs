namespace Arbor.Graphics.Textures;

public interface ITextureUpload : IDisposable
{
    /// <summary>
    /// The raw data to be uploaded.
    /// </summary>
    ReadOnlySpan<Rgba32> Data { get; }
    
    /// <summary>
    /// The target bounds for this upload. If not specified, will be assume to be (0, 0, width, height).
    /// </summary>
    Rectangle Bounds { get; set; }
}
