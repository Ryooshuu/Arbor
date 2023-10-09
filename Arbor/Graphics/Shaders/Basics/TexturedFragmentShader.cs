using Veldrid;

namespace Arbor.Graphics.Shaders.Basics;

internal  class TexturedFragmentShader : FragmentShader
{
    public TextureView TextureView { get; }
    public Sampler TextureSampler { get; }

    public TexturedFragmentShader(TextureView textureView, Sampler textureSampler)
    {
        TextureView = textureView;
        TextureSampler = textureSampler;
    }

    protected override ShaderDescription CreateShaderDescription()
    {
        var bytes = ReadFromResource(Game.Resources, "Shaders/texture2d.fsh");
        return new ShaderDescription(ShaderStages.Fragment, bytes, "main");
    }
    
    public override BindableResource[] CreateBindableResources()
        => new BindableResource[] { TextureView, TextureSampler };
    
    public override ResourceLayoutElementDescription[] CreateResourceDescriptions()
        => new ResourceLayoutElementDescription[]
        {
            new ResourceLayoutElementDescription("in_Texture", ResourceKind.TextureReadOnly, Stage),
            new ResourceLayoutElementDescription("in_TextureSampler", ResourceKind.Sampler, Stage),
        };

    
    protected override void Dispose(bool disposing)
    {
        TextureView.Dispose();
        TextureSampler.Dispose();
    }
}
