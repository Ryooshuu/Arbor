using GlmSharp;
using Veldrid;

namespace Arbor.Graphics.Shaders.Vertices;

public struct VertexUvColour
{
    [VertexDescription("in_Position", VertexElementFormat.Float2)]
    public vec2 Position;
    [VertexDescription("in_Uv", VertexElementFormat.Float2)]
    public vec2 Uv;
    [VertexDescription("in_Colour", VertexElementFormat.Float4)]
    public RgbaFloat Colour;

    public VertexUvColour(vec2 position, vec2 uv, RgbaFloat colour)
    {
        Position = position;
        Uv = uv;
        Colour = colour;
    }
}
