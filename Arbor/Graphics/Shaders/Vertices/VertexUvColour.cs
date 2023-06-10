using GlmSharp;
using Veldrid;

namespace Arbor.Graphics.Shaders.Vertices;

public struct VertexUvColour
{
    public vec2 Position;
    public vec2 Uv;
    public RgbaFloat Colour;

    public VertexUvColour(vec2 position, vec2 uv, RgbaFloat colour)
    {
        Position = position;
        Uv = uv;
        Colour = colour;
    }
}
