using Veldrid;

namespace Arbor.Graphics.Shaders.Vertices;

[AttributeUsage(AttributeTargets.Field)]
public class VertexDescriptionAttribute : Attribute
{
    /// <summary>
    /// The name of the element.
    /// </summary>
    public string Name;

    /// <summary>
    /// The semantic type of the element.
    /// NOTE: When using Veldrid.SPIRV, all vertex elements will use <see cref="VertexElementSemantic.TextureCoordinate"/>.
    /// </summary>
    public VertexElementSemantic Semantic;
    
    /// <summary>
    /// The format of the element.
    /// </summary>
    public VertexElementFormat Format;

    /// <summary>
    /// The offset in bytes from the beginning of the vertex.
    /// </summary>
    public uint Offset;

    public VertexDescriptionAttribute(string name, VertexElementFormat format)
        : this(name, VertexElementSemantic.TextureCoordinate, format)
    {
    }

    public VertexDescriptionAttribute(string name, VertexElementFormat format, uint offset)
        : this(name, VertexElementSemantic.TextureCoordinate, format, offset)
    {
    }

    public VertexDescriptionAttribute(string name, VertexElementSemantic semantic, VertexElementFormat format, uint offset = 0)
    {
        Name = name;
        Semantic = semantic;
        Format = format;
        Offset = offset;
    }
}
