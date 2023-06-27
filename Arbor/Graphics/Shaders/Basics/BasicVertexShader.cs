using Arbor.Graphics.Shaders.Vertices;
using Arbor.Resources;
using Veldrid;

namespace Arbor.Graphics.Shaders.Basics;

internal class BasicVertexShader : VertexShader<VertexPositionColour>
{
    protected override ShaderDescription CreateShaderDescription()
    {
        var bytes = ReadFromResource("basic.vsh", ArborResources.ResourcesAssembly);
        return new ShaderDescription(ShaderStages.Vertex, bytes, "main");
    }
}
