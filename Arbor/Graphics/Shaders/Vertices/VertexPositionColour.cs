using System.Runtime.InteropServices;
using GlmSharp;
using Veldrid;

namespace Arbor.Graphics.Shaders.Vertices;

[StructLayout(LayoutKind.Sequential)]
public struct VertexPositionColour
{
    [VertexDescription("in_Position", VertexElementFormat.Float2)]
    public vec2 Position;
    [VertexDescription("in_Colour", VertexElementFormat.Float4)]
    public RgbaFloat Color;

    public VertexPositionColour(vec2 position, RgbaFloat color)
    {
        Position = position;
        Color = color;
    }
}
