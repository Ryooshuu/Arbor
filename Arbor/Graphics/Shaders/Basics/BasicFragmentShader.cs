using Arbor.Resources;
using Veldrid;

namespace Arbor.Graphics.Shaders.Basics;

internal class BasicFragmentShader : FragmentShader
{
    protected override ShaderDescription CreateShaderDescription()
    {
        var bytes = ReadFromResource("basic.fsh", ArborResources.ResourcesAssembly);
        return new ShaderDescription(ShaderStages.Fragment, bytes, "main");
    }
}
