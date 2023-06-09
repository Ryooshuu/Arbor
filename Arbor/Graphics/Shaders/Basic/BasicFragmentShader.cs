using Arbor.Resources;
using Veldrid;

namespace Arbor.Graphics.Shaders.Basic;

public class BasicFragmentShader : Shader
{
    public override ShaderStages Stage => ShaderStages.Fragment;

    public override ShaderDescription CreateShaderDescription()
    {
        var bytes = ReadFromResource("basic.fsh", ArborResources.ResourcesAssembly);
        return new ShaderDescription(Stage, bytes, "main");
    }
}
