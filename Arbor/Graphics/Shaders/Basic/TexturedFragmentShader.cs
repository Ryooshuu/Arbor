using Arbor.Resources;
using Veldrid;

namespace Arbor.Graphics.Shaders.Basic;

public class TexturedFragmentShader : Shader
{
    public override ShaderStages Stage => ShaderStages.Fragment;

    public TextureView? TextureView { private get; set; }
    public Sampler? TextureSampler { private get; set; }
    
    public override ShaderDescription CreateShaderDescription()
    {
        var bytes = ReadFromResource("texture2d.fsh", ArborResources.ResourcesAssembly);
        return new ShaderDescription(Stage, bytes, "main");
    }

    public override BindableResource[] CreateBindableResources()
    {
        if (TextureView == null)
            throw new NullReferenceException($"{nameof(TextureView)} cannot be null.");
        if (TextureSampler == null)
            throw new NullReferenceException($"{nameof(TextureSampler)} cannot be null.");

        return new BindableResource[] { TextureView, TextureSampler };
    }

    public override ResourceLayoutElementDescription[] CreateResourceDescriptions()
        => new ResourceLayoutElementDescription[]
        {
            new("in_Texture", ResourceKind.TextureReadOnly, Stage),
            new("in_TextureSampler", ResourceKind.Sampler, Stage),
        };
}
