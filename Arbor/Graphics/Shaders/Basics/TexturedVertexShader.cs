using Arbor.Graphics.Shaders.Vertices;
using Arbor.Resources;
using Veldrid;

namespace Arbor.Graphics.Shaders.Basics;

internal class TexturedVertexShader : VertexShader<VertexUvColour>
{
    protected override ShaderDescription CreateShaderDescription()
    {
        var bytes = ReadFromResource("texture2d.vsh", ArborResources.ResourcesAssembly);
        return new ShaderDescription(ShaderStages.Vertex, bytes, "main");
    }
}
