using Veldrid;

namespace Arbor.Graphics.Shaders.Basics;

internal class BasicFragmentShader : FragmentShader
{
    protected override ShaderDescription CreateShaderDescription()
    {
        var bytes = ReadFromResource(Game.Resources, "Shaders/basic.fsh");
        return new ShaderDescription(ShaderStages.Fragment, bytes, "main");
    }
}
