namespace Arbor.Graphics.Textures;

/// <summary>
/// A texture which can clean up any resources held by the underlying <see cref="Veldrid.Texture"/> on <see cref="Dispose"/>
/// </summary>
public class DisposableTexture : Texture
{
    private readonly Texture parent;

    public DisposableTexture(Texture parent)
        : base(parent)
    {
        this.parent = parent;
    }

    protected override void Dispose(bool isDisposing)
    {
        base.Dispose(isDisposing);
        
        NativeTexture.Dispose();
        parent.Dispose();
    }
}
