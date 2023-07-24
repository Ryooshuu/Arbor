using Arbor.Graphics.Shaders.Vertices;
using Veldrid;

namespace Arbor.Graphics.Shaders.Basics;

internal class TexturedVertexShader : VertexShader<VertexUvColour>
{
    protected override ShaderDescription CreateShaderDescription()
    {
        var bytes = ReadFromResource(Game.Resources, "Shaders/texture2d.vsh");
        return new ShaderDescription(ShaderStages.Vertex, bytes, "main");
    }
}
