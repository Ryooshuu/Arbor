using Arbor.Resources;
using Veldrid;

namespace Arbor.Graphics.Shaders.Basic;

public class BasicVertexShader : Shader
{
    public override ShaderStages Stage => ShaderStages.Vertex;

    public override ShaderDescription CreateShaderDescription()
    {
        var bytes = ReadFromResource("basic.vsh", ArborResources.ResourcesAssembly);
        return new ShaderDescription(Stage, bytes, "main");
    }

    public override VertexElementDescription[] CreateVertexDescriptions()
        => new VertexElementDescription[]
        {
            new("in_Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new("in_Colour", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4)
        };
}
