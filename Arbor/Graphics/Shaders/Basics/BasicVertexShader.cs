using Arbor.Graphics.Shaders.Vertices;
using Veldrid;

namespace Arbor.Graphics.Shaders.Basics;

internal class BasicVertexShader : VertexShader<VertexPositionColour>
{
    protected override ShaderDescription CreateShaderDescription()
    {
        var bytes = ReadFromResource(Game.Resources, "Shaders/basic.vsh");
        return new ShaderDescription(ShaderStages.Vertex, bytes, "main");
    }
}
